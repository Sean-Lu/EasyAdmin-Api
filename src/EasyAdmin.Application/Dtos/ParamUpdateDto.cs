using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

[AutoMap(typeof(ParamEntity), ReverseMap = true)]
public class ParamUpdateDto : DtoIdBase
{
    public string ParamName { get; set; }
    public string ParamKey { get; set; }
    public string ParamValue { get; set; }
    public string? Remark { get; set; }
    public CommonState State { get; set; }
}