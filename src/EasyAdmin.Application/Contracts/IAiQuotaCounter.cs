namespace EasyAdmin.Application.Contracts;

/// <summary>
/// AI配额计数器
/// </summary>
public interface IAiQuotaCounter
{
    /// <summary>
    /// 预占一次请求配额
    /// </summary>
    Task ReserveAsync(long tenantId, int limit, DateTime now);

    /// <summary>
    /// 获取当日已用配额
    /// </summary>
    Task<long> GetUsedAsync(long tenantId, DateTime now);
}