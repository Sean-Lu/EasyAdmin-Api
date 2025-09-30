using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Tenant;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Application.Services;

public class CheckInLogService(
    ILogger<CheckInLogService> logger,
    IMapper mapper,
    ICheckInLogRepository checkInLogRepository
    ) : ICheckInLogService
{
    public async Task<bool> AddAsync(CheckInLogDto dto)
    {
        var entity = mapper.Map<CheckInLogEntity>(dto);
        entity.UserId = TenantContextHolder.UserId;
        return await checkInLogRepository.AddAsync(entity);
    }

    public async Task<List<CheckInLogEntity>?> SearchAsync(long userId, CheckInType checkInType, int year, int month)
    {
        var startTime = new DateTime(year, month, 1, 0, 0, 0);
        var endTime = startTime.AddMonths(1);
        return (await checkInLogRepository.QueryAsync(entity => entity.UserId == userId && entity.CheckInType == checkInType && entity.CheckInTime >= startTime && entity.CreateTime < endTime))?.ToList();
    }

    public async Task<bool> IsCheckInTodayAsync(long userId, CheckInType checkInType)
    {
        var time = DateTime.Now;
        var startTime = time.Date;
        var endTime = time.Date.AddDays(1);
        return await checkInLogRepository.ExistsAsync(entity => entity.UserId == userId && entity.CheckInType == checkInType && entity.CheckInTime >= startTime && entity.CreateTime < endTime);
    }
}