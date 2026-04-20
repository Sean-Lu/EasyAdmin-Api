using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Domain.Extensions;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Helper;
using EasyAdmin.Infrastructure.Tenant;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

public class DepartmentService(
    ILogger<DepartmentService> logger,
    IMapper mapper,
    IDepartmentRepository departmentRepository
    ) : IDepartmentService
{
    public async Task<bool> AddAsync(DepartmentDto dto)
    {
        var entity = mapper.Map<DepartmentEntity>(dto);
        return await departmentRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await departmentRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await departmentRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(DepartmentDto dto)
    {
        return await departmentRepository.UpdateByDtoAsync(dto, mapper.Map<DepartmentEntity>) > 0;
    }

    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await departmentRepository.UpdateAsync(new DepartmentEntity { State = state }, entity => new { entity.State }, entity => entity.Id == id) > 0;
    }

    public async Task<List<DepartmentEntity>> GetDepartmentTreeAsync(DepartmentListReqDto request)
    {
        var list = (await departmentRepository.QueryAsync(WhereExpressionUtil.Create<DepartmentEntity>(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Name), entity => entity.Name.Contains(request.Name))
            .AndAlsoIF(!request.All, entity => entity.State == CommonState.Enable)))?.ToList() ?? new List<DepartmentEntity>();
        if (list.Any() && !list.Exists(c => c.PId == SysConst.TopDepartmentId))
        {
            // 向上查找所有上级部门
            await TreeHelper.AddAllParentsAsync(
                list,
                async (id) => await departmentRepository.GetByIdAsync(id),
                entity => entity.Id,
                entity => entity.PId,
                entity => entity.PId == SysConst.TopDepartmentId
            );
        }
        var treeList = list.ToTreeList(SysConst.TopDepartmentId);
        if (request.IncludeTopDepartment)
        {
            return new List<DepartmentEntity>
            {
                new()
                {
                    Id = 0,
                    PId = SysConst.TopDepartmentId,
                    Name = SysConst.TopDepartmentName,
                    Children = treeList
                }
            };
        }
        return treeList;
    }

    public async Task<DepartmentEntity> GetByIdAsync(long id)
    {
        var entity = await departmentRepository.GetByIdAsync(id);
        if (entity.Id > 0)
        {
            entity.ParentFullPath = await GetParentFullPathAsync(entity);
        }
        return entity;
    }

    private async Task<string> GetParentFullPathAsync(DepartmentEntity entity)
    {
        if (entity.PId == 0)
        {
            return SysConst.TopDepartmentName;
        }
        if (entity.PId == entity.Id)
        {
            return entity.Name;
        }

        var parent = await departmentRepository.GetByIdAsync(entity.PId);
        var parentFullPath = await GetParentFullPathAsync(parent);
        return $"{parentFullPath} / {parent.Name}";
    }
}
