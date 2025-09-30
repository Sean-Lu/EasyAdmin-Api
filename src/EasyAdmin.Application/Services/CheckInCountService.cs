using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Tenant;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Application.Services;

public class CheckInCountService(
    ILogger<CheckInCountService> logger,
    IMapper mapper,
    ICheckInCountRepository checkInCountRepository
    ) : ICheckInCountService
{
    public async Task<bool> AddAsync(CheckInCountDto dto)
    {
        var entity = mapper.Map<CheckInCountEntity>(dto);
        entity.UserId = TenantContextHolder.UserId;
        return await checkInCountRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await checkInCountRepository.DeleteByIdAsync(id);
    }
    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await checkInCountRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(CheckInCountDto dto)
    {
        var entity = mapper.Map<CheckInCountEntity>(dto);
        return await checkInCountRepository.UpdateAsync(entity) > 0;
    }

    public async Task<bool> IncrAsync(long userId, CheckInType checkInType, DateTime lastCheckInTime, int incrCount, bool firstDay = false)
    {
        if (userId < 1 || incrCount < 0)
        {
            return false;
        }

        var updateModel = new CheckInCountEntity
        {
            LastCheckInTime = lastCheckInTime
        };

        return await checkInCountRepository.ExecuteAutoTransactionAsync(async tran =>
        {
            if (await checkInCountRepository.UpdateAsync(updateModel, entity => entity.LastCheckInTime, entity => entity.UserId == userId && entity.CheckInType == checkInType, tran) < 1)
            {
                return false;
            }

            if (firstDay)
            {
                updateModel.ContinuousCheckInDays = 1;
                if (await checkInCountRepository.UpdateAsync(updateModel, entity => entity.ContinuousCheckInDays, entity => entity.UserId == userId && entity.CheckInType == checkInType, tran) < 1)
                {
                    return false;
                }
            }
            else
            {
                if (!await checkInCountRepository.IncrementAsync(incrCount, entity => entity.ContinuousCheckInDays, entity => entity.UserId == userId && entity.CheckInType == checkInType, tran))
                {
                    return false;
                }
            }

            return true;
        });
    }

    public async Task<bool> InitContinuousCheckInDaysAsync(long userId, CheckInType checkInType)
    {
        if (userId < 1)
        {
            return false;
        }

        return await checkInCountRepository.UpdateAsync(new CheckInCountEntity
        {
            ContinuousCheckInDays = 0
        }, entity => entity.ContinuousCheckInDays, entity => entity.UserId == userId && entity.CheckInType == checkInType) > 0;
    }

    public async Task<CheckInCountEntity> GetByIdAsync(long id)
    {
        return await checkInCountRepository.GetByIdAsync(id);
    }

    public async Task<CheckInCountEntity> GetAsync(long userId, CheckInType checkInType)
    {
        return await checkInCountRepository.GetAsync(entity => entity.UserId == userId && entity.CheckInType == checkInType);
    }
}