using EasyAdmin.Application.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Domain.Contracts;
using Microsoft.Extensions.Logging;
using EasyAdmin.Application.Dtos;
using Sean.Core.DbRepository.Util;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using EasyAdmin.Infrastructure.Tenant;
using MapsterMapper;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Application.Services;

public class FileService(
    ILogger<FileService> logger,
    IMapper mapper,
    IFileRepository fileRepository
) : IFileService
{
    public static bool CanDeleteFromFileManager(FileBizType bizType)
    {
        return bizType == FileBizType.Normal;
    }

    public async Task<bool> AddAsync(FileDto dto)
    {
        var entity = mapper.Map<FileEntity>(dto);
        return await fileRepository.AddAsync(entity);
    }

    public async Task<long> AddAndReturnIdAsync(FileDto dto)
    {
        var entity = mapper.Map<FileEntity>(dto);
        return await fileRepository.AddAsync(entity) ? entity.Id : 0;
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await fileRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> DeleteByIdFromFileManagerAsync(long id)
    {
        var entity = await fileRepository.GetByIdAsync(id);
        if (entity == null || entity.Id < 1)
        {
            return false;
        }
        if (!CanDeleteFromFileManager(entity.BizType))
        {
            throw new ExplicitException("该文件被业务引用，不能在文件管理中删除");
        }
        return await fileRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await fileRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> HasOtherActiveFileWithSamePathAsync(long id, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var file = await fileRepository.GetAsync(entity =>
            entity.Id != id &&
            entity.TenantId == TenantContextHolder.TenantId &&
            entity.Path == path &&
            !entity.IsDelete);
        return file != null && file.Id > 0;
    }

    public async Task<PageQueryResult<FileEntity>> PageAsync(FilePageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<FileEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        orderBy.Next = OrderByConditionBuilder<FileEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await fileRepository.PageQueryAsync(WhereExpressionUtil.Create<FileEntity>(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Name), entity => entity.Name.Contains(request.Name))
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Description), entity => entity.Description.Contains(request.Description))
            .AndAlsoIF(request.BizType.HasValue, entity => entity.BizType == request.BizType.Value), orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<FileEntity> GetByIdAsync(long id)
    {
        return await fileRepository.GetByIdAsync(id);
    }
}
