using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Tenant;
using Elastic.Clients.Elasticsearch;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 待办事项服务实现
/// </summary>
public class TodoItemService(
    IMapper mapper,
    ITodoItemRepository todoItemRepository
    ) : ITodoItemService
{
    public async Task<bool> AddAsync(TodoItemDto dto)
    {
        var entity = mapper.Map<TodoItemEntity>(dto);
        entity.UserId = TenantContextHolder.UserId;
        return await todoItemRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await todoItemRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> ClearCompletedAsync(long? categoryId = null)
    {
        if (categoryId.HasValue)
        {
            await todoItemRepository.DeleteAsync(entity => entity.UserId == TenantContextHolder.UserId && entity.Done && entity.CategoryId == categoryId.Value && entity.TenantId == TenantContextHolder.TenantId);
        }
        else
        {
            await todoItemRepository.DeleteAsync(entity => entity.UserId == TenantContextHolder.UserId && entity.Done && entity.TenantId == TenantContextHolder.TenantId);
        }
        return true;
    }

    public async Task<bool> UpdateAsync(TodoItemDto dto)
    {
        return await todoItemRepository.UpdateByDtoAsync(dto, mapper.Map<TodoItemEntity>) > 0;
    }

    public async Task<bool> UpdateStatusAsync(long id, bool done)
    {
        await todoItemRepository.UpdateAsync(new TodoItemEntity { Done = done }, entity => entity.Done, entity => entity.Id == id);
        return true;
    }

    public async Task<bool> UpdateStatusAsync(List<long> ids, bool done)
    {
        await todoItemRepository.UpdateAsync(new TodoItemEntity { Done = done }, entity => entity.Done, entity => ids.Contains(entity.Id));
        return true;
    }

    public async Task<bool> UpdatePriorityAsync(long id, int priority)
    {
        await todoItemRepository.UpdateAsync(new TodoItemEntity { Priority = priority }, entity => entity.Priority, entity => entity.Id == id);
        return true;
    }

    public async Task<bool> UpdateNameAsync(long id, string name)
    {
        await todoItemRepository.UpdateAsync(new TodoItemEntity { Name = name }, entity => entity.Name, entity => entity.Id == id);
        return true;
    }

    public async Task<bool> UpdateSortOrderAsync(long id, int sortOrder)
    {
        await todoItemRepository.UpdateAsync(new TodoItemEntity { SortOrder = sortOrder }, entity => entity.SortOrder, entity => entity.Id == id);
        return true;
    }

    public async Task<bool> UpdateCategoryAsync(long id, long categoryId)
    {
        await todoItemRepository.UpdateAsync(new TodoItemEntity { CategoryId = categoryId }, entity => entity.CategoryId, entity => entity.Id == id);
        return true;
    }

    public async Task<TodoItemDto> GetByIdAsync(long id)
    {
        var entity = await todoItemRepository.GetByIdAsync(id);
        return mapper.Map<TodoItemDto>(entity);
    }

    public async Task<List<TodoItemDto>> GetByUserIdAsync(long categoryId)
    {
        var orderBy = OrderByConditionBuilder<TodoItemEntity>.Build(OrderByType.Asc, entity => entity.Done,
            OrderByConditionBuilder<TodoItemEntity>.Build(OrderByType.Desc, entity => entity.Priority,
                OrderByConditionBuilder<TodoItemEntity>.Build(OrderByType.Asc, entity => entity.SortOrder,
                    OrderByConditionBuilder<TodoItemEntity>.Build(OrderByType.Desc, entity => entity.CreateTime,
                        OrderByConditionBuilder<TodoItemEntity>.Build(OrderByType.Desc, entity => entity.Id)))));
        var entities = (await todoItemRepository.QueryAsync(entity => entity.UserId == TenantContextHolder.UserId && entity.CategoryId == categoryId && entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete, orderBy))?.ToList();
        return mapper.Map<List<TodoItemDto>>(entities);
    }
}