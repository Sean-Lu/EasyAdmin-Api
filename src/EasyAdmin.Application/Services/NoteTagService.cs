using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Tenant;
using MapsterMapper;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 笔记标签服务
/// </summary>
public class NoteTagService(
    IMapper mapper,
    INoteTagRepository noteTagRepository,
    INoteTagRelationRepository noteTagRelationRepository
    ) : INoteTagService
{
    public async Task<List<NoteTagDto>> GetByUserIdAsync()
    {
        var orderBy = OrderByConditionBuilder<NoteTagEntity>.Build(OrderByType.Desc, entity => entity.UseCount,
            OrderByConditionBuilder<NoteTagEntity>.Build(OrderByType.Asc, entity => entity.Name));
        var entities = (await noteTagRepository.QueryAsync(entity =>
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete, orderBy))?.ToList() ?? new List<NoteTagEntity>();
        return mapper.Map<List<NoteTagDto>>(entities);
    }

    public async Task<List<NoteTagDto>> SuggestAsync(string? keyword)
    {
        keyword = keyword?.Trim() ?? string.Empty;
        var orderBy = OrderByConditionBuilder<NoteTagEntity>.Build(OrderByType.Desc, entity => entity.UseCount,
            OrderByConditionBuilder<NoteTagEntity>.Build(OrderByType.Asc, entity => entity.Name));
        var entities = (await noteTagRepository.QueryAsync(entity =>
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete &&
            (string.IsNullOrEmpty(keyword) || entity.Name.Contains(keyword)), orderBy))?.Take(20).ToList() ?? new List<NoteTagEntity>();
        return mapper.Map<List<NoteTagDto>>(entities);
    }

    public async Task<bool> DeleteUnusedAsync()
    {
        var relations = (await noteTagRelationRepository.QueryAsync(entity =>
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete,
            fieldExpression: entity => new { entity.TagId }))?.ToList() ?? new List<NoteTagRelationEntity>();
        var usedTagIds = relations.Select(entity => entity.TagId).Distinct().ToList();
        await noteTagRepository.DeleteAsync(entity =>
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !usedTagIds.Contains(entity.Id));
        return true;
    }
}
