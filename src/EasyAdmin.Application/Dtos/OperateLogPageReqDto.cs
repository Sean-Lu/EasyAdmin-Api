namespace EasyAdmin.Application.Dtos;

public class OperateLogPageReqDto : PageRequestBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long? UserId { get; set; }
    /// <summary>
    /// IP地址
    /// </summary>
    public string? IP { get; set; }
}