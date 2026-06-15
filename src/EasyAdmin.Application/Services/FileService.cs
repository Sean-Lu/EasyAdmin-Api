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

namespace EasyAdmin.Application.Services;

public class FileService(
    ILogger<FileService> logger,
    IMapper mapper,
    IFileRepository fileRepository
) : IFileService
{
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
    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await fileRepository.DeleteByIdsAsync(ids);
    }

    public async Task<PageQueryResult<FileEntity>> PageAsync(FilePageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<FileEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        orderBy.Next = OrderByConditionBuilder<FileEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await fileRepository.PageQueryAsync(WhereExpressionUtil.Create<FileEntity>(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Name), entity => entity.Name.Contains(request.Name))
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Description), entity => entity.Description.Contains(request.Description)), orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<FileEntity> GetByIdAsync(long id)
    {
        return await fileRepository.GetByIdAsync(id);
    }
}