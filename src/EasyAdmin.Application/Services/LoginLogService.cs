using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Tenant;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository.Util;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;

namespace EasyAdmin.Application.Services;

public class LoginLogService(
    ILogger<LoginLogService> logger,
    IMapper mapper,
    ILoginLogRepository loginLogRepository
    ) : ILoginLogService
{
    public async Task<bool> AddAsync(LoginLogDto dto)
    {
        var entity = mapper.Map<LoginLogEntity>(dto);
        return await loginLogRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await loginLogRepository.DeleteByIdAsync(id);
    }
    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await loginLogRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(LoginLogDto dto)
    {
        var entity = mapper.Map<LoginLogEntity>(dto);
        return await loginLogRepository.UpdateAsync(entity) > 0;
    }

    public async Task<PageQueryResult<LoginLogEntity>> PageAsync(LoginLogPageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<LoginLogEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        orderBy.Next = OrderByConditionBuilder<LoginLogEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await loginLogRepository.PageQueryAsync(WhereExpressionUtil.Create<LoginLogEntity>(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete)
            .AndAlsoIF(request.UserId.HasValue && request.UserId.Value > 0, entity => entity.UserId == request.UserId.GetValueOrDefault())
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.IP), entity => entity.IP.Contains(request.IP)), orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<LoginLogEntity> GetByIdAsync(long id)
    {
        return await loginLogRepository.GetByIdAsync(id);
    }
}