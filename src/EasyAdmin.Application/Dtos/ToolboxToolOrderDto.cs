namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 百宝箱工具排序
/// </summary>
public class ToolboxToolOrderDto
{
    /// <summary>
    /// 工具ID
    /// </summary>
    public List<long> ToolIds { get; set; } = new();
}
