namespace EasyAdmin.Application.Dtos;

public class FilePageReqDto : PageRequestBase
{
    /// <summary>
    /// 文件名称
    /// </summary>
    public virtual string? Name { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    public virtual string? Description { get; set; }
}