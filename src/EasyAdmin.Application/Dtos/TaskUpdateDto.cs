using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

[AutoMap(typeof(TaskEntity), ReverseMap = true)]
public class TaskUpdateDto : DtoIdBase
{
    public long UserId { get; set; }
    public string TaskName { get; set; }
    public int TaskType { get; set; }
    public string? TaskReward { get; set; }
    public DateTime? TaskStartTime { get; set; }
    public DateTime? TaskEndTime { get; set; }
    public string? TaskRule { get; set; }
    public CommonState State { get; set; }
}