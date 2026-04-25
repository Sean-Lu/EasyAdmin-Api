using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface IWeekWorkReportService
{
    Task<bool> AddAsync(WeekWorkReportDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(WeekWorkReportDto dto);
    Task<PageQueryResult<WeekWorkReportEntity>> PageAsync(WeekWorkReportPageReqDto request);
    Task<WeekWorkReportEntity> GetByIdAsync(long id);
}
