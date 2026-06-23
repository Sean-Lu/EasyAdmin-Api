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
    INotePasswordService notePasswordService,
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
            OrderByConditionBuilder<NoteEntity>.Build(OrderByType.Desc, entity => entity.LastEditTime,
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
        var result = await noteRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            var now = DateTime.Now;
            var entity = mapper.Map<NoteEntity>(dto);
            entity.UserId = TenantContextHolder.UserId;
            entity.CategoryId = await NormalizeCategoryIdAsync(dto.CategoryId);
            entity.ContentHtml = NoteContentHelper.SanitizeHtml(dto.ContentHtml);
            entity.ContentText = NoteContentHelper.ExtractText(entity.ContentHtml);
            entity.Summary = NoteContentHelper.CreateSummary(entity.ContentText);
            entity.LastEditTime = now;
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

        var oldImageIds = NoteContentHelper.ExtractImageFileIds(note.ContentHtml);
        var nextContentHtml = NoteContentHelper.SanitizeHtml(dto.ContentHtml);
        var nextImageIds = NoteContentHelper.ExtractImageFileIds(nextContentHtml);
        var result = await noteRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            var entity = mapper.Map<NoteEntity>(dto);
            entity.CategoryId = await NormalizeCategoryIdAsync(dto.CategoryId);
            entity.ContentHtml = nextContentHtml;
            entity.ContentText = NoteContentHelper.ExtractText(entity.ContentHtml);
            entity.Summary = NoteContentHelper.CreateSummary(entity.ContentText);
            entity.LastEditTime = DateTime.Now;
            await noteRepository.UpdateAsync(entity,
                update => new { update.CategoryId, update.Title, update.ContentHtml, update.ContentText, update.Summary, update.IsTop, update.IsProtected, update.LastEditTime },
                noteEntity => noteEntity.Id == dto.Id && noteEntity.UserId == TenantContextHolder.UserId && noteEntity.TenantId == TenantContextHolder.TenantId,
                transaction);
            await SaveTagsAsync(dto.Id, dto.Tags, transaction);
            return true;
        });
        await RefreshTagUseCountAsync();
        await DeleteUnusedNoteImagesAsync(oldImageIds.Except(nextImageIds));
        return result;
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        var note = await GetCurrentUserNoteAsync(id);
        var imageIds = NoteContentHelper.ExtractImageFileIds(note?.ContentHtml);
        var result = await noteRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            await noteTagRelationRepository.DeleteAsync(entity => entity.NoteId == id && entity.TenantId == TenantContextHolder.TenantId, transaction);
            await noteRepository.DeleteAsync(entity => entity.Id == id && entity.UserId == TenantContextHolder.UserId && entity.TenantId == TenantContextHolder.TenantId, transaction);
            return true;
        });
        await RefreshTagUseCountAsync();
        await DeleteUnusedNoteImagesAsync(imageIds);
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
        var imageIds = notes.SelectMany(entity => NoteContentHelper.ExtractImageFileIds(entity.ContentHtml)).Distinct().ToList();
        var result = await noteRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            await noteTagRelationRepository.DeleteAsync(entity => ids.Contains(entity.NoteId) && entity.TenantId == TenantContextHolder.TenantId, transaction);
            await noteRepository.DeleteAsync(entity => ids.Contains(entity.Id) && entity.UserId == TenantContextHolder.UserId && entity.TenantId == TenantContextHolder.TenantId, transaction);
            return true;
        });
        await RefreshTagUseCountAsync();
        await DeleteUnusedNoteImagesAsync(imageIds);
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
            .SelectMany(entity => NoteContentHelper.ExtractImageFileIds(entity.ContentHtml))
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
        return activeNotes.Any(entity => NoteContentHelper.ExtractImageFileIds(entity.ContentHtml).Contains(id));
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
}
