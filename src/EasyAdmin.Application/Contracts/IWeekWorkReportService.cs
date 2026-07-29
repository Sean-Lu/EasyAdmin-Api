using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 周报服务接口
/// </summary>
public interface IWeekWorkReportService
{
    Task<bool> AddAsync(WeekWorkReportDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(WeekWorkReportUpdateDto dto);
    Task<PageQueryResult<WeekWorkReportEntity>> PageAsync(WeekWorkReportPageReqDto request);
    Task<WeekWorkReportEntity> GetByIdAsync(long id);
}
