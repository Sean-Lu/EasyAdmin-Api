using EasyAdmin.Application.Contracts;
using EasyAdmin.Domain.Entities;
using AutoMapper;
using EasyAdmin.Domain.Contracts;
using Microsoft.Extensions.Logging;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Repositories;
using Sean.Core.DbRepository.Util;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using EasyAdmin.Infrastructure.Tenant;

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

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await fileRepository.DeleteByIdAsync(id);
    }
    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await fileRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(FileDto dto)
    {
        var entity = mapper.Map<FileEntity>(dto);
        return await fileRepository.UpdateAsync(entity) > 0;
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