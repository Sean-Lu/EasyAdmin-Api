using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 部门服务接口
/// </summary>
public interface IDepartmentService
{
    Task<bool> AddAsync(DepartmentDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(DepartmentUpdateDto dto);
    Task<bool> UpdateStateAsync(long id, CommonState state);
    Task<List<DepartmentEntity>> GetDepartmentTreeAsync(DepartmentListReqDto request);
    Task<DepartmentEntity> GetByIdAsync(long id);
}
