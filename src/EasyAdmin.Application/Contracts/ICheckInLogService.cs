using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Contracts;

public interface ICheckInLogService
{
    Task<bool> AddAsync(CheckInLogDto dto);
    Task<List<CheckInLogEntity>?> SearchAsync(long userId, CheckInType checkInType, int year, int month);
    Task<bool> IsCheckInTodayAsync(long userId, CheckInType checkInType);
}