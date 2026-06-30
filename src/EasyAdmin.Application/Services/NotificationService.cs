using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

public class NotificationService(
    ILogger<NotificationService> logger,
    IMapper mapper,
    INotificationRepository notificationRepository,
    IUserNotificationRepository userNotificationRepository,
    INotificationChannelDispatcher notificationChannelDispatcher,
    IUserRepository userRepository,
    IUserRoleRepository userRoleRepository,
    IDepartmentRepository departmentRepository
    ) : INotificationService
{
    public async Task<bool> AddAsync(NotificationDto dto)
    {
        var users = (await userRepository.QueryAsync(user => user.TenantId == TenantContextHolder.TenantId && !user.IsDelete))?.ToList() ?? new List<UserEntity>();
        var userRoles = (await userRoleRepository.QueryAsync(userRole => userRole.TenantId == TenantContextHolder.TenantId && !userRole.IsDelete))?.ToList() ?? new List<UserRoleEntity>();
        var departments = (await departmentRepository.QueryAsync(department => department.TenantId == TenantContextHolder.TenantId && !department.IsDelete))?.ToList() ?? new List<DepartmentEntity>();
        var recipientUserIds = NotificationRecipientResolver.Resolve(dto, users, userRoles, departments, TenantContextHolder.TenantId, TenantContextHolder.UserId);
        if (!recipientUserIds.Any())
        {
            throw new ExplicitException("没有匹配到可接收用户");
        }

        var notification = mapper.Map<NotificationEntity>(dto);
        notification.SenderUserId = TenantContextHolder.UserId;
        notification.SendTime = DateTime.Now;
        notification.State = CommonState.Enable;
        notification.TargetSummary = BuildTargetSummary(dto, recipientUserIds.Count);

        if (!await notificationRepository.AddAsync(notification))
        {
            return false;
        }

        if (dto.SendInSystem)
        {
            // 生成站内消息
            foreach (var userId in recipientUserIds)
            {
                if (!await userNotificationRepository.AddAsync(new UserNotificationEntity
                {
                    NotificationId = notification.Id,
                    UserId = userId,
                    IsRead = false
                }))
                {
                    logger.LogWarning("保存用户消息失败: NotificationId={NotificationId}, UserId={UserId}", notification.Id, userId);
                    return false;
                }
            }
        }

        // 分发外部通道
        await notificationChannelDispatcher.DispatchAsync(new NotificationChannelDispatchRequest
        {
            Title = dto.Title,
            Content = dto.Content,
            SendEmail = dto.SendEmail,
            SendSms = dto.SendSms,
            Recipients = users.Where(user => recipientUserIds.Contains(user.Id)).ToList()
        });

        return true;
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await notificationRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await notificationRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await notificationRepository.UpdateAsync(new NotificationEntity { State = state }, entity => new { entity.State }, entity => entity.Id == id && entity.TenantId == TenantContextHolder.TenantId) > 0;
    }

    public async Task<PageQueryResult<NotificationEntity>> PageAsync(NotificationPageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<NotificationEntity>.Build(OrderByType.Desc, entity => entity.SendTime);
        orderBy.Next = OrderByConditionBuilder<NotificationEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await notificationRepository.PageQueryAsync(WhereExpressionUtil.Create<NotificationEntity>(entity => entity.SenderUserId == TenantContextHolder.UserId && entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Title), entity => entity.Title.Contains(request.Title))
            .AndAlsoIF(request.NoticeType.HasValue, entity => entity.NoticeType == request.NoticeType), orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<NotificationEntity> GetByIdAsync(long id)
    {
        return await notificationRepository.GetAsync(entity => entity.Id == id && entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete);
    }

    public async Task<PageQueryResult<UserNotificationEntity>> UserMessagePageAsync(UserMessagePageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<UserNotificationEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        orderBy.Next = OrderByConditionBuilder<UserNotificationEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await userNotificationRepository.PageQueryAsync(WhereExpressionUtil.Create<UserNotificationEntity>(entity =>
                entity.TenantId == TenantContextHolder.TenantId &&
                entity.UserId == TenantContextHolder.UserId &&
                !entity.IsDelete)
            .AndAlsoIF(request.IsRead.HasValue, entity => entity.IsRead == request.IsRead.Value)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Title), entity => entity.Title.Contains(request.Title)), orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<UserNotificationEntity?> GetUserMessageDetailAsync(long id)
    {
        var message = await userNotificationRepository.GetAsync(entity =>
            entity.Id == id &&
            entity.TenantId == TenantContextHolder.TenantId &&
            entity.UserId == TenantContextHolder.UserId &&
            !entity.IsDelete);
        if (message == null || message.Id < 1)
        {
            return null;
        }

        if (!message.IsRead)
        {
            await userNotificationRepository.UpdateAsync(new UserNotificationEntity
            {
                IsRead = true,
                ReadTime = DateTime.Now
            }, entity => new { entity.IsRead, entity.ReadTime }, entity => entity.Id == id && entity.UserId == TenantContextHolder.UserId && entity.TenantId == TenantContextHolder.TenantId);

            message.IsRead = true;
            message.ReadTime = DateTime.Now;
        }

        return message;
    }

    public async Task<int> GetUnreadCountAsync()
    {
        var unreadMessages = await userNotificationRepository.QueryAsync(entity =>
            entity.TenantId == TenantContextHolder.TenantId &&
            entity.UserId == TenantContextHolder.UserId &&
            !entity.IsRead &&
            !entity.IsDelete);
        return unreadMessages?.Count() ?? 0;
    }

    public async Task<List<UserNotificationEntity>> GetRecentUnreadAsync(int count)
    {
        count = count <= 0 ? 5 : Math.Min(count, 20);
        var orderBy = OrderByConditionBuilder<UserNotificationEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        var page = await userNotificationRepository.PageQueryAsync(WhereExpressionUtil.Create<UserNotificationEntity>(entity =>
            entity.TenantId == TenantContextHolder.TenantId &&
            entity.UserId == TenantContextHolder.UserId &&
            !entity.IsRead &&
            !entity.IsDelete), orderBy, 1, count);
        return page.List ?? new List<UserNotificationEntity>();
    }

    public async Task<bool> MarkReadAsync(List<long> ids)
    {
        if (ids == null || !ids.Any())
        {
            return true;
        }

        return await userNotificationRepository.UpdateAsync(new UserNotificationEntity
        {
            IsRead = true,
            ReadTime = DateTime.Now
        }, entity => new { entity.IsRead, entity.ReadTime }, entity =>
            ids.Contains(entity.Id) &&
            entity.TenantId == TenantContextHolder.TenantId &&
            entity.UserId == TenantContextHolder.UserId &&
            !entity.IsDelete) >= 0;
    }

    public async Task<bool> MarkAllReadAsync()
    {
        return await userNotificationRepository.UpdateAsync(new UserNotificationEntity
        {
            IsRead = true,
            ReadTime = DateTime.Now
        }, entity => new { entity.IsRead, entity.ReadTime }, entity =>
            entity.TenantId == TenantContextHolder.TenantId &&
            entity.UserId == TenantContextHolder.UserId &&
            !entity.IsRead &&
            !entity.IsDelete) >= 0;
    }

    private static string BuildTargetSummary(NotificationDto dto, int recipientCount)
    {
        var parts = new List<string>();
        if (dto.SendToAll)
        {
            parts.Add("全员");
        }
        if (dto.UserIds?.Any() == true)
        {
            parts.Add($"指定用户{dto.UserIds.Distinct().Count()}个");
        }
        if (dto.RoleIds?.Any() == true)
        {
            parts.Add($"角色{dto.RoleIds.Distinct().Count()}个");
        }
        if (dto.DepartmentIds?.Any() == true)
        {
            parts.Add($"部门{dto.DepartmentIds.Distinct().Count()}个");
        }

        return $"{string.Join("、", parts)}；接收人{recipientCount}个";
    }
}
