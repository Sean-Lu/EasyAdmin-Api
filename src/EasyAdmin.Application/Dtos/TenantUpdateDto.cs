using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

[AutoMap(typeof(TenantEntity), ReverseMap = true)]
public class TenantUpdateDto : DtoIdBase
{
    public string Name { get; set; }
    public CommonState State { get; set; }
    public string? Remark { get; set; }
}