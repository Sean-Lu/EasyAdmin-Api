using AutoMapper;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 操作日志表
/// </summary>
[AutoMap(typeof(OperateLogEntity), ReverseMap = true)]
public class OperateLogDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    /// <summary>
    /// 用户昵称
    /// </summary>
    public virtual string UserNickName { get; set; }
    /// <summary>
    /// 操作内容
    /// </summary>
    public virtual string? Content { get; set; }
    /// <summary>
    /// IP地址
    /// </summary>
    public virtual string? IP { get; set; }
}