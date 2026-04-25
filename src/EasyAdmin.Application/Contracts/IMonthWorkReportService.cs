using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface IMonthWorkReportService
{
    Task<bool> AddAsync(MonthWorkReportDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(MonthWorkReportDto dto);
    Task<PageQueryResult<MonthWorkReportEntity>> PageAsync(MonthWorkReportPageReqDto request);
    Task<MonthWorkReportEntity> GetByIdAsync(long id);
}
