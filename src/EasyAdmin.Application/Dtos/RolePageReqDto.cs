namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 角色列表分页查询条件
/// </summary>
public class RolePageReqDto : PageRequestBase
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// 角色编码
    /// </summary>
    public string? Code { get; set; }
}