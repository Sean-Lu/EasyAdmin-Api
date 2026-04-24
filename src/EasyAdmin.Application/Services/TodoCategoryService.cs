using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Tenant;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 待办事项分类服务实现
/// </summary>
public class TodoCategoryService(
    IMapper mapper,
    ITodoCategoryRepository todoCategoryRepository,
    ITodoItemRepository todoItemRepository
    ) : ITodoCategoryService
{
    public async Task<bool> AddAsync(TodoCategoryDto dto)
    {
        var entity = mapper.Map<TodoCategoryEntity>(dto);
        entity.UserId = TenantContextHolder.UserId;
        return await todoCategoryRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await todoCategoryRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            // 先删除该分类下的所有待办事项
            await todoItemRepository.DeleteAsync(entity => entity.CategoryId == id && entity.UserId == TenantContextHolder.UserId, transaction);
            // 再删除分类
            await todoCategoryRepository.DeleteByIdAsync(id, transaction);
            return true;
        });
    }

    public async Task<bool> UpdateAsync(TodoCategoryUpdateDto dto)
    {
        return await todoCategoryRepository.UpdateByDtoAsync(dto, mapper.Map<TodoCategoryEntity>) > 0;
    }

    public async Task<TodoCategoryDto> GetByIdAsync(long id)
    {
        var entity = await todoCategoryRepository.GetByIdAsync(id);
        return mapper.Map<TodoCategoryDto>(entity);
    }

    public async Task<List<TodoCategoryDto>> GetByUserIdAsync()
    {
        var orderBy = OrderByConditionBuilder<TodoCategoryEntity>.Build(OrderByType.Asc, entity => entity.SortOrder,
            OrderByConditionBuilder<TodoCategoryEntity>.Build(OrderByType.Asc, entity => entity.CreateTime));
        var entities = (await todoCategoryRepository.QueryAsync(entity => entity.UserId == TenantContextHolder.UserId && entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete, orderBy))?.ToList();

        // 如果用户没有分类，创建一个默认分类
        if (entities == null || entities.Count == 0)
        {
            var defaultCategory = new TodoCategoryEntity
            {
                UserId = TenantContextHolder.UserId,
                Name = "默认分类",
                SortOrder = 0,
            };
            await todoCategoryRepository.AddAsync(defaultCategory);
            entities = new List<TodoCategoryEntity> { defaultCategory };
        }

        return mapper.Map<List<TodoCategoryDto>>(entities);
    }
}
