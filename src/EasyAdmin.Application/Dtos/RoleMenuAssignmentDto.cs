namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 角色菜单权限分配
/// </summary>
public class RoleMenuAssignmentDto
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public long RoleId { get; set; }
    
    /// <summary>
    /// 菜单ID列表
    /// </summary>
    public List<long> MenuIds { get; set; } = new List<long>();
}