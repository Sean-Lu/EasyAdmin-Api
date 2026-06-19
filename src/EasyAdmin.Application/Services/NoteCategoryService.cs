using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Tenant;
using MapsterMapper;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 笔记分类服务
/// </summary>
public class NoteCategoryService(
    IMapper mapper,
    INoteCategoryRepository noteCategoryRepository,
    INoteRepository noteRepository
    ) : INoteCategoryService
{
    public async Task<bool> AddAsync(NoteCategoryDto dto)
    {
        var entity = mapper.Map<NoteCategoryEntity>(dto);
        entity.UserId = TenantContextHolder.UserId;
        return await noteCategoryRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await noteCategoryRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            await noteRepository.UpdateAsync(new NoteEntity { CategoryId = 0 }, entity => entity.CategoryId,
                entity => entity.CategoryId == id && entity.UserId == TenantContextHolder.UserId && entity.TenantId == TenantContextHolder.TenantId, transaction);
            await noteCategoryRepository.DeleteByIdAsync(id, transaction);
            return true;
        });
    }

    public async Task<bool> UpdateAsync(NoteCategoryUpdateDto dto)
    {
        return await noteCategoryRepository.UpdateByDtoAsync(dto, mapper.Map<NoteCategoryEntity>) > 0;
    }

    public async Task<bool> UpdateSortOrderAsync(long id, int sortOrder)
    {
        return await noteCategoryRepository.UpdateAsync(new NoteCategoryEntity { SortOrder = sortOrder }, entity => entity.SortOrder,
            entity => entity.Id == id && entity.UserId == TenantContextHolder.UserId && entity.TenantId == TenantContextHolder.TenantId) > 0;
    }

    public async Task<List<NoteCategoryDto>> GetByUserIdAsync()
    {
        var orderBy = OrderByConditionBuilder<NoteCategoryEntity>.Build(OrderByType.Asc, entity => entity.SortOrder,
            OrderByConditionBuilder<NoteCategoryEntity>.Build(OrderByType.Asc, entity => entity.CreateTime));
        var entities = (await noteCategoryRepository.QueryAsync(entity =>
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete, orderBy))?.ToList() ?? new List<NoteCategoryEntity>();

        if (entities.Count == 0)
        {
            var defaultCategory = new NoteCategoryEntity
            {
                UserId = TenantContextHolder.UserId,
                Name = "默认分类",
                SortOrder = 0
            };
            await noteCategoryRepository.AddAsync(defaultCategory);
            entities.Add(defaultCategory);
        }

        var dtos = mapper.Map<List<NoteCategoryDto>>(entities);
        var notes = (await noteRepository.QueryAsync(entity =>
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete,
            fieldExpression: entity => new { entity.CategoryId }))?.ToList() ?? new List<NoteEntity>();
        var countDict = notes.GroupBy(entity => entity.CategoryId).ToDictionary(group => group.Key, group => group.Count());

        foreach (var dto in dtos)
        {
            countDict.TryGetValue(dto.Id, out var count);
            dto.NoteCount = count;
        }

        return dtos;
    }
}
