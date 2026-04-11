namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 租户列表分页查询条件
/// </summary>
public class TenantPageReqDto : PageRequestBase
{
    /// <summary>
    /// 租户名称
    /// </summary>
    public string? Name { get; set; }
}