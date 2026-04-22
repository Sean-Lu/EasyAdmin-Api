using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 字典数据 DTO
/// </summary>
[AutoMap(typeof(SysDictDataEntity), ReverseMap = true)]
public class SysDictDataDto : DtoBase
{
    public virtual long DictTypeId { get; set; }
    public virtual int DictKey { get; set; }
    public virtual string DictValue { get; set; }
    public virtual int Sort { get; set; }
    public virtual CommonState State { get; set; }
}