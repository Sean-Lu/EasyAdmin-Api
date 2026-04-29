using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

[AutoMap(typeof(SysDictDataEntity), ReverseMap = true)]
public class SysDictDataUpdateDto : DtoIdBase
{
    public long DictTypeId { get; set; }
    public int DictKey { get; set; }
    public string DictValue { get; set; }
    public int Sort { get; set; }
    public CommonState State { get; set; }
}