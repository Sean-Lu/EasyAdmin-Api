using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface IDayWorkReportService
{
    Task<bool> AddAsync(DayWorkReportDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(DayWorkReportDto dto);
    Task<PageQueryResult<DayWorkReportEntity>> PageAsync(DayWorkReportPageReqDto request);
    Task<DayWorkReportEntity> GetByIdAsync(long id);
}