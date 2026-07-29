using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 签到记录仓储实现
/// </summary>
public class CheckInLogRepository(IConfiguration configuration, ILogger<CheckInLogRepository> logger) : BaseRepositoryExt<CheckInLogEntity>(configuration, logger), ICheckInLogRepository
{
    /// <inheritdoc />
    public async Task<List<CheckInLogEntity>?> SearchAsync(long userId, int pageNumber, int pageSize)
    {
        var orderBy = OrderByConditionBuilder<CheckInLogEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        return (await QueryAsync(entity => entity.UserId == userId && !entity.IsDelete, orderBy, pageNumber, pageSize))?.ToList();
    }
}