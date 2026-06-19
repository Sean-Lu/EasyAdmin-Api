namespace EasyAdmin.Application.Dtos;

using EasyAdmin.Infrastructure.Enums;

/// <summary>
/// 文件列表分页查询条件
/// </summary>
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
    /// <summary>
    /// 业务分类
    /// </summary>
    public virtual FileBizType? BizType { get; set; }
}
