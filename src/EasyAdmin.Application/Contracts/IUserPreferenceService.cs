using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 用户偏好服务接口
/// </summary>
public interface IUserPreferenceService
{
    /// <summary>
    /// 获取百宝箱工具排序
    /// </summary>
    Task<ToolboxToolOrderDto> GetToolboxToolOrderAsync();

    /// <summary>
    /// 更新百宝箱工具排序
    /// </summary>
    Task<ToolboxToolOrderDto> UpdateToolboxToolOrderAsync(ToolboxToolOrderDto request);
}
