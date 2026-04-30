namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 角色菜单权限 DTO
/// </summary>
public class RoleMenuDto
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public virtual long RoleId { get; set; }

    /// <summary>
    /// 菜单ID
    /// </summary>
    public virtual long MenuId { get; set; }
}