using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

[AutoMap(typeof(RoleEntity), ReverseMap = true)]
public class RoleUpdateDto : DtoIdBase
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Description { get; set; }
    public int Sort { get; set; }
    public CommonState State { get; set; }
}