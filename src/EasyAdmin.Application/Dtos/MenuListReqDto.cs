namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 菜单列表查询条件
/// </summary>
public class MenuListReqDto
{
    /// <summary>
    /// 是否查询所有菜单（包含被禁用的）
    /// </summary>
    public bool All { get; set; }
    /// <summary>
    /// 是否包含顶级菜单
    /// </summary>
    public bool IncludeTopMenu { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string? Title { get; set; }
    /// <summary>
    /// 菜单路由
    /// </summary>
    public string? Path { get; set; }
}