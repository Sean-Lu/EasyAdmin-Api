using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface INotificationService
{
    Task<bool> AddAsync(NotificationDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateStateAsync(long id, CommonState state);
    Task<PageQueryResult<NotificationEntity>> PageAsync(NotificationPageReqDto request);
    Task<NotificationEntity> GetByIdAsync(long id);
    Task<PageQueryResult<UserNotificationEntity>> UserMessagePageAsync(UserMessagePageReqDto request);
    Task<UserNotificationEntity?> GetUserMessageDetailAsync(long id);
    Task<int> GetUnreadCountAsync();
    Task<List<UserNotificationEntity>> GetRecentUnreadAsync(int count);
    Task<bool> MarkReadAsync(List<long> ids);
    Task<bool> MarkAllReadAsync();
}
