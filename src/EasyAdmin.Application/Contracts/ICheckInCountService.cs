using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Contracts;

public interface ICheckInCountService
{
    Task<bool> AddAsync(CheckInCountDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(CheckInCountDto dto);
    /// <summary>
    /// 增加连续签到天数，并更新最后签到时间
    /// </summary>
    /// <param name="userId">用户id</param>
    /// <param name="checkInType">签到类型</param>
    /// <param name="lastCheckInTime">最后签到时间</param>
    /// <param name="incrCount">增加的天数，可以为0（不更新天数）</param>
    /// <param name="firstDay">表示连续签到天数是否中断</param>
    /// <returns></returns>
    Task<bool> IncrAsync(long userId, CheckInType checkInType, DateTime lastCheckInTime, int incrCount, bool firstDay = false);
    /// <summary>
    /// 初始化连续签到天数
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="checkInType"></param>
    /// <returns></returns>
    Task<bool> InitContinuousCheckInDaysAsync(long userId, CheckInType checkInType);
    Task<CheckInCountEntity> GetByIdAsync(long id);
    /// <summary>
    /// 获取签到统计信息
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="checkInType"></param>
    /// <returns></returns>
    Task<CheckInCountEntity> GetAsync(long userId, CheckInType checkInType);
}