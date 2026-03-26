using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Contracts;

public interface IDepartmentService
{
    Task<bool> AddAsync(DepartmentDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(DepartmentDto dto);
    Task<bool> UpdateStateAsync(long id, CommonState state);
    Task<List<DepartmentEntity>> GetDepartmentTreeAsync(DepartmentListReqDto request);
    Task<DepartmentEntity> GetByIdAsync(long id);
}
