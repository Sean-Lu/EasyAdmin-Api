namespace EasyAdmin.Application.Dtos;

public class TenantPageReqDto : PageRequestBase
{
    /// <summary>
    /// 租户名称
    /// </summary>
    public string? Name { get; set; }
}