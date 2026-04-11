namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 用户角色分配
/// </summary>
public class UserRoleAssignmentDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// 角色ID列表
    /// </summary>
    public List<long> RoleIds { get; set; } = new List<long>();
}