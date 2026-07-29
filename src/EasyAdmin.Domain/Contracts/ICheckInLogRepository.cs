using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Domain.Contracts;

/// <summary>
/// 签到记录仓储接口
/// </summary>
public interface ICheckInLogRepository : IBaseRepositoryExt<CheckInLogEntity>
{
    /// <summary>
    /// 分页查询用户签到记录
    /// </summary>
    Task<List<CheckInLogEntity>?> SearchAsync(long userId, int pageNumber, int pageSize);
}