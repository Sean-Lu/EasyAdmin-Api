using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 股票账户服务接口
/// </summary>
public interface IStockAccountService
{
    /// <summary>
    /// 新增股票账户
    /// </summary>
    Task<bool> AddAsync(StockAccountDto dto);

    /// <summary>
    /// 删除股票账户
    /// </summary>
    Task<bool> DeleteByIdAsync(long id);

    /// <summary>
    /// 更新股票账户基础资产信息
    /// </summary>
    Task<bool> UpdateAsync(StockAccountUpdateDto dto);

    /// <summary>
    /// 获取当前用户的股票账户列表
    /// </summary>
    Task<List<StockAccountDto>> ListAsync();

    /// <summary>
    /// 判断账户是否属于当前用户和当前租户
    /// </summary>
    Task<bool> ExistsForCurrentUserAsync(long id);
}
