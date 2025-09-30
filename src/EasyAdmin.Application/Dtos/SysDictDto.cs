using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 字典表
/// </summary>
[AutoMap(typeof(SysDictEntity), ReverseMap = true)]
public class SysDictDto : TreeDtoBase<SysDictDto>
{
    /// <summary>
    /// 编码
    /// </summary>
    public virtual string Code { get; set; }
    /// <summary>
    /// 字段键
    /// </summary>
    public virtual int DictKey { get; set; }
    /// <summary>
    /// 字段值
    /// </summary>
    public virtual string DictValue { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string Remark { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    public virtual CommonState State { get; set; }
}