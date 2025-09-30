using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Domain.Contracts;

public interface ICheckInLogRepository : IBaseRepositoryExt<CheckInLogEntity>
{
    Task<List<CheckInLogEntity>?> SearchAsync(long userId, int pageNumber, int pageSize);
}