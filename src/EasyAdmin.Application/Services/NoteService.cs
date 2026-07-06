using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Storage;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;
using System.IO.Compression;
using System.Text;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 笔记服务
/// </summary>
public class NoteService(
    ILogger<NoteService> logger,
    IMapper mapper,
    INoteRepository noteRepository,
    INoteCategoryRepository noteCategoryRepository,
    INoteTagRepository noteTagRepository,
    INoteTagRelationRepository noteTagRelationRepository,
    INoteTagService noteTagService,
    INotePasswordService notePasswordService,
    INoteMarkdownService noteMarkdownService,
    IFileService fileService,
    IFileStorageFactory fileStorageFactory
    ) : INoteService
{
    public async Task<PageQueryResult<NoteDto>> PageAsync(NotePageReqDto request)
    {
        var tagNoteIds = await GetNoteIdsByTagsAsync(request.TagIds);
        if (request.TagIds?.Any() == true && tagNoteIds.Count == 0)
        {
            return new PageQueryResult<NoteDto> { Total = 0, List = new List<NoteDto>() };
        }

        var keyword = request.Keyword?.Trim();
        var orderBy = OrderByConditionBuilder<NoteEntity>.Build(OrderByType.Desc, entity => entity.IsTop,
            OrderByConditionBuilder<NoteEntity>.Build(OrderByType.Desc, entity => entity.UpdateTime,
                OrderByConditionBuilder<NoteEntity>.Build(OrderByType.Desc, entity => entity.Id)));
        var page = await noteRepository.PageQueryAsync(WhereExpressionUtil.Create<NoteEntity>(entity =>
                entity.UserId == TenantContextHolder.UserId &&
                entity.TenantId == TenantContextHolder.TenantId &&
                !entity.IsDelete)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(keyword), entity =>
                entity.Title.Contains(keyword!) ||
                (entity.ContentText != null && entity.ContentText.Contains(keyword!)) ||
                (entity.Summary != null && entity.Summary.Contains(keyword!)))
            .AndAlsoIF(request.CategoryId.HasValue, entity => entity.CategoryId == request.CategoryId!.Value)
            .AndAlsoIF(request.IsProtected.HasValue, entity => entity.IsProtected == request.IsProtected!.Value)
            .AndAlsoIF(request.IsTop.HasValue, entity => entity.IsTop == request.IsTop!.Value)
            .AndAlsoIF(request.StartTime.HasValue, entity => entity.CreateTime >= request.StartTime!.Value)
            .AndAlsoIF(request.EndTime.HasValue, entity => entity.CreateTime <= request.EndTime!.Value)
            .AndAlsoIF(request.TagIds?.Any() == true, entity => tagNoteIds.Contains(entity.Id)),
            orderBy, request.PageNumber, request.PageSize);

        var dtos = await BuildNoteDtosAsync(page.List?.ToList() ?? new List<NoteEntity>());
        foreach (var dto in dtos)
        {
            dto.ContentHtml = null;
            dto.ContentMarkdown = null;
            if (dto.IsProtected)
            {
                dto.Summary = null;
                dto.ContentText = null;
            }
        }

        return new PageQueryResult<NoteDto>
        {
            Total = page.Total,
            List = dtos
        };
    }

    public async Task<NoteDto?> GetDetailAsync(long id, string? unlockToken)
    {
        var note = await GetCurrentUserNoteAsync(id);
        if (note == null || note.Id < 1)
        {
            return null;
        }

        if (note.IsProtected && !await notePasswordService.ValidateUnlockTokenAsync(unlockToken))
        {
            throw new ExplicitException("请输入笔记密码");
        }

        return (await BuildNoteDtosAsync(new List<NoteEntity> { note })).FirstOrDefault();
    }

    public async Task<bool> AddAsync(NoteUpdateDto dto)
    {
        var content = BuildContent(dto);
        await ValidateImageIdsAsync(content.ImageIds);
        var result = await noteRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            var now = DateTime.Now;
            var entity = mapper.Map<NoteEntity>(dto);
            entity.UserId = TenantContextHolder.UserId;
            entity.CategoryId = await NormalizeCategoryIdAsync(dto.CategoryId);
            entity.ContentMarkdown = content.ContentMarkdown;
            entity.ContentHtml = content.ContentHtml;
            entity.ContentText = NoteContentHelper.ExtractText(content.ContentHtml);
            entity.Summary = NoteContentHelper.CreateSummary(entity.ContentText);
            await noteRepository.AddAsync(entity, transaction: transaction);
            await SaveTagsAsync(entity.Id, dto.Tags, transaction);
            return true;
        });
        await RefreshTagUseCountAsync();
        return result;
    }

    public async Task<bool> UpdateAsync(NoteUpdateDto dto)
    {
        var note = await GetCurrentUserNoteAsync(dto.Id);
        if (note == null || note.Id < 1)
        {
            return false;
        }

        if (note.ContentType != dto.ContentType)
        {
            throw new ExplicitException("笔记正文格式不能修改");
        }
        var oldImageIds = GetImageIds(note);
        var content = BuildContent(dto);
        await ValidateImageIdsAsync(content.ImageIds);
        var result = await noteRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            var entity = mapper.Map<NoteEntity>(dto);
            entity.CategoryId = await NormalizeCategoryIdAsync(dto.CategoryId);
            entity.ContentMarkdown = content.ContentMarkdown;
            entity.ContentHtml = content.ContentHtml;
            entity.ContentText = NoteContentHelper.ExtractText(entity.ContentHtml);
            entity.Summary = NoteContentHelper.CreateSummary(entity.ContentText);
            await noteRepository.UpdateAsync(entity,
                update => new { update.CategoryId, update.Title, update.ContentType, update.ContentMarkdown, update.ContentHtml, update.ContentText, update.Summary, update.IsTop, update.IsProtected },
                noteEntity => noteEntity.Id == dto.Id && noteEntity.UserId == TenantContextHolder.UserId && noteEntity.TenantId == TenantContextHolder.TenantId,
                transaction);
            await SaveTagsAsync(dto.Id, dto.Tags, transaction);
            return true;
        });
        await RefreshTagUseCountAsync();
        await DeleteUnusedNoteImagesAsync(oldImageIds.Except(content.ImageIds));
        await noteTagService.DeleteUnusedAsync();
        return result;
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        var note = await GetCurrentUserNoteAsync(id);
        var imageIds = note == null ? new List<long>() : GetImageIds(note);
        var result = await noteRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            await noteTagRelationRepository.DeleteAsync(entity => entity.NoteId == id && entity.TenantId == TenantContextHolder.TenantId, transaction);
            await noteRepository.DeleteAsync(entity => entity.Id == id && entity.UserId == TenantContextHolder.UserId && entity.TenantId == TenantContextHolder.TenantId, transaction);
            return true;
        });
        await RefreshTagUseCountAsync();
        await DeleteUnusedNoteImagesAsync(imageIds);
        await noteTagService.DeleteUnusedAsync();
        return result;
    }

    public async Task<bool> BatchDeleteAsync(List<long> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            return true;
        }

        var notes = (await noteRepository.QueryAsync(entity =>
            ids.Contains(entity.Id) &&
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete))?.ToList() ?? new List<NoteEntity>();
        var imageIds = notes.SelectMany(GetImageIds).Distinct().ToList();
        var result = await noteRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            await noteTagRelationRepository.DeleteAsync(entity => ids.Contains(entity.NoteId) && entity.TenantId == TenantContextHolder.TenantId, transaction);
            await noteRepository.DeleteAsync(entity => ids.Contains(entity.Id) && entity.UserId == TenantContextHolder.UserId && entity.TenantId == TenantContextHolder.TenantId, transaction);
            return true;
        });
        await RefreshTagUseCountAsync();
        await DeleteUnusedNoteImagesAsync(imageIds);
        await noteTagService.DeleteUnusedAsync();
        return result;
    }

    public async Task<bool> DeleteImageFileAsync(long id)
    {
        if (id < 1)
        {
            return true;
        }
        if (await IsNoteImageReferencedAsync(id))
        {
            throw new ExplicitException("图片已被笔记引用，不能直接删除");
        }
        return await DeleteNoteImageFileAsync(id);
    }

    public async Task<bool> UpdateTopAsync(long id, bool isTop)
    {
        return await noteRepository.UpdateAsync(new NoteEntity { IsTop = isTop }, entity => entity.IsTop,
            entity => entity.Id == id && entity.UserId == TenantContextHolder.UserId && entity.TenantId == TenantContextHolder.TenantId) > 0;
    }

    public async Task<bool> MoveCategoryAsync(long id, long categoryId)
    {
        return await MoveCategoryAsync(new List<long> { id }, categoryId);
    }

    public async Task<bool> MoveCategoryAsync(List<long> ids, long categoryId)
    {
        if (ids == null || ids.Count == 0)
        {
            return true;
        }

        var newCategoryId = await NormalizeCategoryIdAsync(categoryId);
        return await noteRepository.UpdateAsync(new NoteEntity { CategoryId = newCategoryId }, entity => entity.CategoryId,
            entity => ids.Contains(entity.Id) && entity.UserId == TenantContextHolder.UserId && entity.TenantId == TenantContextHolder.TenantId) > 0;
    }

    private async Task<NoteEntity?> GetCurrentUserNoteAsync(long id)
    {
        return await noteRepository.GetAsync(entity =>
            entity.Id == id &&
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete);
    }

    private async Task<List<NoteDto>> BuildNoteDtosAsync(List<NoteEntity> notes)
    {
        if (notes.Count == 0)
        {
            return new List<NoteDto>();
        }

        var categoryIds = notes.Select(entity => entity.CategoryId).Where(id => id > 0).Distinct().ToList();
        var categories = categoryIds.Count == 0
            ? new List<NoteCategoryEntity>()
            : (await noteCategoryRepository.QueryAsync(entity => categoryIds.Contains(entity.Id) && entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete))?.ToList() ?? new List<NoteCategoryEntity>();
        var categoryDict = categories.ToDictionary(entity => entity.Id, entity => entity.Name);

        var noteIds = notes.Select(entity => entity.Id).ToList();
        var relations = (await noteTagRelationRepository.QueryAsync(entity =>
            noteIds.Contains(entity.NoteId) &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete))?.ToList() ?? new List<NoteTagRelationEntity>();
        var tagIds = relations.Select(entity => entity.TagId).Distinct().ToList();
        var tags = tagIds.Count == 0
            ? new List<NoteTagEntity>()
            : (await noteTagRepository.QueryAsync(entity => tagIds.Contains(entity.Id) && entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete))?.ToList() ?? new List<NoteTagEntity>();
        var tagDict = tags.ToDictionary(entity => entity.Id, mapper.Map<NoteTagDto>);

        var dtos = mapper.Map<List<NoteDto>>(notes);
        foreach (var dto in dtos)
        {
            dto.CategoryName = categoryDict.TryGetValue(dto.CategoryId, out var categoryName) ? categoryName : "未分类";
            dto.Tags = relations.Where(entity => entity.NoteId == dto.Id && tagDict.ContainsKey(entity.TagId))
                .Select(entity => tagDict[entity.TagId])
                .ToList();
        }

        return dtos;
    }

    private async Task<long> NormalizeCategoryIdAsync(long categoryId)
    {
        if (categoryId > 0)
        {
            var category = await noteCategoryRepository.GetAsync(entity =>
                entity.Id == categoryId &&
                entity.UserId == TenantContextHolder.UserId &&
                entity.TenantId == TenantContextHolder.TenantId &&
                !entity.IsDelete);
            if (category != null && category.Id > 0)
            {
                return categoryId;
            }
        }

        var defaultCategory = await noteCategoryRepository.GetAsync(entity =>
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete);
        if (defaultCategory != null && defaultCategory.Id > 0)
        {
            return defaultCategory.Id;
        }

        var entity = new NoteCategoryEntity
        {
            UserId = TenantContextHolder.UserId,
            Name = "默认分类",
            SortOrder = 0
        };
        await noteCategoryRepository.AddAsync(entity);
        return entity.Id;
    }

    private async Task SaveTagsAsync(long noteId, List<string>? tagNames, System.Data.IDbTransaction transaction)
    {
        var names = (tagNames ?? new List<string>())
            .Select(name => name.Trim())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToList();

        await noteTagRelationRepository.DeleteAsync(entity => entity.NoteId == noteId && entity.TenantId == TenantContextHolder.TenantId, transaction);
        foreach (var name in names)
        {
            var tag = await noteTagRepository.GetAsync(entity =>
                entity.UserId == TenantContextHolder.UserId &&
                entity.TenantId == TenantContextHolder.TenantId &&
                entity.Name == name &&
                !entity.IsDelete);
            if (tag == null || tag.Id < 1)
            {
                tag = new NoteTagEntity
                {
                    UserId = TenantContextHolder.UserId,
                    Name = name
                };
                await noteTagRepository.AddAsync(tag, transaction: transaction);
            }

            await noteTagRelationRepository.AddAsync(new NoteTagRelationEntity
            {
                NoteId = noteId,
                TagId = tag.Id
            }, transaction: transaction);
        }
    }

    private async Task RefreshTagUseCountAsync()
    {
        var tags = (await noteTagRepository.QueryAsync(entity =>
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete))?.ToList() ?? new List<NoteTagEntity>();
        if (tags.Count == 0)
        {
            return;
        }

        var tagIds = tags.Select(entity => entity.Id).ToList();
        var relations = (await noteTagRelationRepository.QueryAsync(entity =>
            tagIds.Contains(entity.TagId) &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete,
            fieldExpression: entity => new { entity.TagId }))?.ToList() ?? new List<NoteTagRelationEntity>();
        var countDict = relations.GroupBy(entity => entity.TagId).ToDictionary(group => group.Key, group => group.Count());
        foreach (var tag in tags)
        {
            countDict.TryGetValue(tag.Id, out var count);
            await noteTagRepository.UpdateAsync(new NoteTagEntity { UseCount = count }, entity => entity.UseCount, entity => entity.Id == tag.Id);
        }
    }

    private async Task<List<long>> GetNoteIdsByTagsAsync(List<long>? tagIds)
    {
        if (tagIds == null || tagIds.Count == 0)
        {
            return new List<long>();
        }

        var distinctTagIds = tagIds.Distinct().ToList();
        var relations = (await noteTagRelationRepository.QueryAsync(entity =>
            distinctTagIds.Contains(entity.TagId) &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete))?.ToList() ?? new List<NoteTagRelationEntity>();
        return relations.GroupBy(entity => entity.NoteId)
            .Where(group => distinctTagIds.All(tagId => group.Any(entity => entity.TagId == tagId)))
            .Select(group => group.Key)
            .ToList();
    }

    private async Task DeleteUnusedNoteImagesAsync(IEnumerable<long> candidateIds)
    {
        var ids = candidateIds.Where(id => id > 0).Distinct().ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var activeNotes = (await noteRepository.QueryAsync(entity =>
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete))?.ToList() ?? new List<NoteEntity>();
        var referencedIds = activeNotes
            .SelectMany(GetImageIds)
            .Distinct()
            .ToHashSet();

        foreach (var id in ids.Where(id => !referencedIds.Contains(id)))
        {
            await DeleteNoteImageFileAsync(id);
        }
    }

    private async Task<bool> IsNoteImageReferencedAsync(long id)
    {
        var activeNotes = (await noteRepository.QueryAsync(entity =>
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete))?.ToList() ?? new List<NoteEntity>();
        return activeNotes.Any(entity => GetImageIds(entity).Contains(id));
    }

    private async Task<bool> DeleteNoteImageFileAsync(long id)
    {
        var file = await fileService.GetByIdAsync(id);
        if (file == null || file.Id < 1)
        {
            return true;
        }
        if (file.BizType != FileBizType.NoteImage || file.TenantId != TenantContextHolder.TenantId || file.CreateUserId != TenantContextHolder.UserId)
        {
            return false;
        }

        try
        {
            if (!await fileService.HasOtherActiveFileWithSamePathAsync(id, file.Path))
            {
                await fileStorageFactory.GetFileStorage(file.StoreType).DeleteAsync(file.Path);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "删除笔记图片文件失败");
        }
        return await fileService.DeleteByIdAsync(id);
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportMarkdownAsync(long id, string? unlockToken, bool includeImages)
    {
        var note = await GetDetailAsync(id, unlockToken);
        if (note == null || note.ContentType != NoteContentType.Markdown)
        {
            throw new ExplicitException("仅Markdown笔记支持此导出");
        }
        var title = NormalizeFileName(note.Title);
        var markdown = note.ContentMarkdown ?? string.Empty;
        if (!includeImages)
        {
            return (new UTF8Encoding(false).GetBytes(markdown), "text/markdown; charset=utf-8", $"{title}.md");
        }

        var replacements = new Dictionary<long, string>();
        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, true, Encoding.UTF8))
        {
            foreach (var idValue in noteMarkdownService.ExtractImageFileIds(markdown))
            {
                var file = await fileService.GetByIdAsync(idValue);
                if (file == null || file.BizType != FileBizType.NoteImage || file.TenantId != TenantContextHolder.TenantId || file.CreateUserId != TenantContextHolder.UserId)
                {
                    throw new ExplicitException("笔记包含无权访问的图片");
                }
                var extension = Path.GetExtension(file.Name).ToLowerInvariant();
                var entryName = $"images/image-{idValue}{extension}";
                replacements[idValue] = entryName;
                var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
                await using var source = await fileStorageFactory.GetFileStorage(file.StoreType).DownloadAsync(file.Path);
                await using var target = entry.Open();
                await source.CopyToAsync(target);
            }
            var markdownEntry = archive.CreateEntry($"{title}.md", CompressionLevel.Fastest);
            await using var writer = new StreamWriter(markdownEntry.Open(), new UTF8Encoding(false));
            await writer.WriteAsync(noteMarkdownService.RewriteImageReferences(markdown, replacements));
        }
        return (output.ToArray(), "application/zip", $"{title}.zip");
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> BatchExportMarkdownAsync(
        IEnumerable<long> ids,
        string? unlockToken,
        bool includeImages)
    {
        var files = new List<(byte[] Content, string FileName)>();
        foreach (var id in ids.Distinct())
        {
            var file = await ExportMarkdownAsync(id, unlockToken, includeImages);
            files.Add((file.Content, file.FileName));
        }
        return (NoteBatchMarkdownArchiveHelper.Build(files, includeImages), "application/zip", "我的笔记.zip");
    }

    public async Task<NoteMarkdownImportDto> ImportMarkdownPackageAsync(Stream stream, string fileName)
    {
        if (stream.Length > 20 * 1024 * 1024)
        {
            throw new ExplicitException("Markdown资源包不能超过20MB");
        }
        var uploadedIds = new List<long>();
        try
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true, Encoding.UTF8);
            if (archive.Entries.Count > 100 || archive.Entries.Any(entry =>
                !NoteMarkdownPackageHelper.IsSafeEntryPath(entry.FullName) || ((entry.ExternalAttributes >> 16) & 0xF000) == 0xA000))
            {
                throw new ExplicitException("Markdown资源包包含非法路径或文件过多");
            }
            if (archive.Entries.Sum(entry => entry.Length) > 100L * 1024 * 1024)
            {
                throw new ExplicitException("Markdown资源包解压后内容过大");
            }
            var markdownEntries = archive.Entries.Where(entry => !entry.FullName.Contains('/') && entry.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase)).ToList();
            if (markdownEntries.Count != 1 || markdownEntries[0].Length > 1024 * 1024)
            {
                throw new ExplicitException("资源包根目录必须包含一个不超过1MB的Markdown文件");
            }
            string markdown;
            await using (var markdownStream = markdownEntries[0].Open())
            using (var reader = new StreamReader(markdownStream, new UTF8Encoding(false, true), true))
            {
                markdown = await reader.ReadToEndAsync();
            }
            var entries = archive.Entries.ToDictionary(entry => entry.FullName.Replace('\\', '/'), StringComparer.OrdinalIgnoreCase);
            var replacements = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            foreach (var imagePath in NoteMarkdownPackageHelper.ExtractRelativeImagePaths(markdown))
            {
                if (!entries.TryGetValue(imagePath, out var imageEntry)) continue;
                var extension = Path.GetExtension(imageEntry.Name).ToLowerInvariant();
                if (!new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" }.Contains(extension) || imageEntry.Length > 10 * 1024 * 1024)
                {
                    throw new ExplicitException("资源包包含不支持的图片");
                }
                var storagePath = $"UploadFiles/{TenantContextHolder.TenantId}/{TenantContextHolder.UserId}/{Guid.NewGuid():N}{extension}";
                await using var imageStream = imageEntry.Open();
                using var imageContent = new MemoryStream();
                await imageStream.CopyToAsync(imageContent);
                if (!NoteMarkdownPackageHelper.IsSupportedImageContent(extension, imageContent.ToArray()))
                {
                    throw new ExplicitException("资源包图片内容与扩展名不匹配");
                }
                imageContent.Position = 0;
                var storage = fileStorageFactory.GetFileStorage(FileStoreType.LocalFile);
                var savedPath = await storage.UploadAsync(imageContent, storagePath);
                var imageId = await fileService.AddAndReturnIdAsync(new FileDto
                {
                    Name = imageEntry.Name,
                    Path = savedPath,
                    Size = imageEntry.Length,
                    ContentType = GetImageContentType(extension),
                    StoreType = FileStoreType.LocalFile,
                    BizType = FileBizType.NoteImage,
                    Description = "Markdown资源包图片"
                });
                if (imageId < 1)
                {
                    await storage.DeleteAsync(savedPath);
                    throw new ExplicitException("保存资源包图片失败");
                }
                uploadedIds.Add(imageId);
                replacements[imagePath] = imageId;
            }
            return new NoteMarkdownImportDto
            {
                Title = Path.GetFileNameWithoutExtension(markdownEntries[0].Name),
                ContentMarkdown = NoteMarkdownPackageHelper.RewriteRelativeImages(markdown, replacements),
                UploadedImageIds = uploadedIds
            };
        }
        catch
        {
            foreach (var uploadedId in uploadedIds) await DeleteNoteImageFileAsync(uploadedId);
            throw;
        }
    }

    private NoteContentResult BuildContent(NoteUpdateDto dto)
    {
        if (!Enum.IsDefined(dto.ContentType))
        {
            throw new ExplicitException("不支持的笔记正文格式");
        }
        if (dto.ContentType == NoteContentType.Markdown)
        {
            var markdown = dto.ContentMarkdown ?? string.Empty;
            if (markdown.Length > 1_000_000)
            {
                throw new ExplicitException("Markdown正文过长");
            }
            return new NoteContentResult(markdown, noteMarkdownService.Render(markdown), noteMarkdownService.ExtractImageFileIds(markdown));
        }
        var html = NoteContentHelper.SanitizeHtml(dto.ContentHtml);
        return new NoteContentResult(null, html, NoteContentHelper.ExtractImageFileIds(html));
    }

    private List<long> GetImageIds(NoteEntity note)
    {
        return note.ContentType == NoteContentType.Markdown
            ? noteMarkdownService.ExtractImageFileIds(note.ContentMarkdown).ToList()
            : NoteContentHelper.ExtractImageFileIds(note.ContentHtml);
    }

    private async Task ValidateImageIdsAsync(IEnumerable<long> imageIds)
    {
        foreach (var id in imageIds.Distinct())
        {
            var file = await fileService.GetByIdAsync(id);
            if (file == null || file.Id < 1 || file.BizType != FileBizType.NoteImage ||
                file.TenantId != TenantContextHolder.TenantId || file.CreateUserId != TenantContextHolder.UserId)
            {
                throw new ExplicitException("笔记包含无权访问的图片");
            }
        }
    }

    private sealed record NoteContentResult(string? ContentMarkdown, string ContentHtml, IReadOnlyList<long> ImageIds);

    private static string NormalizeFileName(string fileName)
    {
        var value = string.IsNullOrWhiteSpace(fileName) ? "我的笔记" : fileName.Trim();
        foreach (var invalidChar in Path.GetInvalidFileNameChars()) value = value.Replace(invalidChar, '_');
        return value;
    }

    private static string GetImageContentType(string extension) => extension switch
    {
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        _ => "image/jpeg"
    };
}
