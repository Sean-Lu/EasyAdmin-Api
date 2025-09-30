using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository.Util;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Tenant;

namespace EasyAdmin.Application.Services;

public class TaskService(
    ILogger<TaskService> logger,
    IMapper mapper,
    ITaskRepository taskRepository
    ) : ITaskService
{
    public async Task<bool> AddAsync(TaskDto dto)
    {
        var entity = mapper.Map<TaskEntity>(dto);
        entity.UserId = TenantContextHolder.UserId;
        return await taskRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await taskRepository.DeleteByIdAsync(id);
    }
    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await taskRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(TaskDto dto)
    {
        var entity = mapper.Map<TaskEntity>(dto);
        return await taskRepository.UpdateAsync(entity) > 0;
    }
    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await taskRepository.UpdateAsync(new TaskEntity { State = state }, entity => new { entity.State }, entity => entity.Id == id) > 0;
    }

    public async Task<PageQueryResult<TaskEntity>> PageAsync(TaskPageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<TaskEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        orderBy.Next = OrderByConditionBuilder<TaskEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await taskRepository.PageQueryAsync(WhereExpressionUtil.Create<TaskEntity>(entity => entity.UserId == TenantContextHolder.UserId && !entity.IsDelete)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.TaskName), entity => entity.TaskName.Contains(request.TaskName))
            .AndAlsoIF(request.TaskType.HasValue, entity => entity.TaskType == request.TaskType), orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<TaskEntity> GetByIdAsync(long id)
    {
        return await taskRepository.GetByIdAsync(id);
    }
}