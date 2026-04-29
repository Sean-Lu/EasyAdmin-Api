using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

[AutoMap(typeof(UserEntity), ReverseMap = true)]
public class UserUpdateDto : DtoIdBase
{
    public string UserName { get; set; }
    public string? NickName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public long? DepartmentId { get; set; }
    public long? PositionId { get; set; }
    public CommonState State { get; set; }
}