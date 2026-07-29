using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

/// <summary>
/// AI会话服务实现
/// </summary>
public sealed class AiConversationService(
    IAiConversationRepository conversationRepository,
    IAiMessageRepository messageRepository,
    IAiSourceRepository sourceRepository,
    IAiDraftRepository draftRepository) : IAiConversationService
{
    /// <inheritdoc />
    public async Task<AiConversationDto> CreateAsync()
    {
        var entity = new AiConversationEntity
        {
            TenantId = TenantContextHolder.TenantId,
            UserId = TenantContextHolder.UserId,
            Title = "新会话"
        };
        await conversationRepository.AddAsync(entity);
        return ToConversationDto(entity);
    }

    /// <inheritdoc />
    public async Task<PageQueryResult<AiConversationDto>> PageAsync(AiConversationPageReqDto request)
    {
        var keyword = request.Keyword?.Trim();
        var orderBy = OrderByConditionBuilder<AiConversationEntity>.Build(
            OrderByType.Desc,
            entity => entity.UpdateTime,
            OrderByConditionBuilder<AiConversationEntity>.Build(OrderByType.Desc, entity => entity.Id));
        var page = await conversationRepository.PageQueryAsync(
            WhereExpressionUtil.Create<AiConversationEntity>(entity =>
                    entity.TenantId == TenantContextHolder.TenantId &&
                    entity.UserId == TenantContextHolder.UserId &&
                    !entity.IsDelete)
                .AndAlsoIF(!string.IsNullOrWhiteSpace(keyword), entity => entity.Title.Contains(keyword!)),
            orderBy,
            request.PageNumber,
            request.PageSize);

        return new PageQueryResult<AiConversationDto>
        {
            Total = page.Total,
            List = page.List.Select(ToConversationDto).ToList()
        };
    }

    /// <inheritdoc />
    public async Task<AiConversationDto> RenameAsync(long id, string title)
    {
        var normalizedTitle = title?.Trim() ?? string.Empty;
        if (normalizedTitle.Length is < 1 or > 200)
        {
            throw new ExplicitException("会话标题长度必须在1到200个字符之间");
        }

        var conversation = await GetOwnedAsync(id);
        conversation.Title = normalizedTitle;
        await conversationRepository.UpdateAsync(
            conversation,
            entity => entity.Title,
            entity =>
                entity.Id == id &&
                entity.TenantId == TenantContextHolder.TenantId &&
                entity.UserId == TenantContextHolder.UserId &&
                !entity.IsDelete);
        return ToConversationDto(conversation);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(long id)
    {
        await GetOwnedAsync(id);
        await conversationRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            await sourceRepository.UpdateAsync(
                new AiSourceEntity { IsDelete = true },
                entity => entity.IsDelete,
                entity =>
                    entity.ConversationId == id &&
                    entity.TenantId == TenantContextHolder.TenantId &&
                    entity.UserId == TenantContextHolder.UserId &&
                    !entity.IsDelete,
                transaction);
            await messageRepository.UpdateAsync(
                new AiMessageEntity { IsDelete = true },
                entity => entity.IsDelete,
                entity =>
                    entity.ConversationId == id &&
                    entity.TenantId == TenantContextHolder.TenantId &&
                    entity.UserId == TenantContextHolder.UserId &&
                    !entity.IsDelete,
                transaction);
            await draftRepository.UpdateAsync(
                new AiDraftEntity { IsDelete = true, Status = AiDraftStatus.Deleted },
                entity => new { entity.IsDelete, entity.Status },
                entity =>
                    entity.ConversationId == id &&
                    entity.Status == AiDraftStatus.Pending &&
                    entity.TenantId == TenantContextHolder.TenantId &&
                    entity.UserId == TenantContextHolder.UserId &&
                    !entity.IsDelete,
                transaction);
            await conversationRepository.UpdateAsync(
                new AiConversationEntity { IsDelete = true },
                entity => entity.IsDelete,
                entity =>
                    entity.Id == id &&
                    entity.TenantId == TenantContextHolder.TenantId &&
                    entity.UserId == TenantContextHolder.UserId &&
                    !entity.IsDelete,
                transaction);
            return true;
        });
    }

    /// <inheritdoc />
    public async Task<PageQueryResult<AiMessageDto>> PageMessagesAsync(AiMessagePageReqDto request)
    {
        await GetOwnedAsync(request.ConversationId);
        var orderBy = OrderByConditionBuilder<AiMessageEntity>.Build(
            OrderByType.Desc,
            entity => entity.Sequence,
            OrderByConditionBuilder<AiMessageEntity>.Build(OrderByType.Desc, entity => entity.Id));
        var page = await messageRepository.PageQueryAsync(
            entity =>
                entity.ConversationId == request.ConversationId &&
                entity.TenantId == TenantContextHolder.TenantId &&
                entity.UserId == TenantContextHolder.UserId &&
                !entity.IsDelete,
            orderBy,
            request.PageNumber,
            request.PageSize);
        var messages = page.List.AsEnumerable().Reverse().ToList();
        var messageIds = messages.Select(entity => entity.Id).ToList();
        var sources = messageIds.Count == 0
            ? new List<AiSourceEntity>()
            : (await sourceRepository.QueryAsync(
                entity =>
                    entity.ConversationId == request.ConversationId &&
                    messageIds.Contains(entity.MessageId) &&
                    entity.TenantId == TenantContextHolder.TenantId &&
                    entity.UserId == TenantContextHolder.UserId &&
                    !entity.IsDelete,
                orderBy: OrderByConditionBuilder<AiSourceEntity>.Build(OrderByType.Asc, entity => entity.Id)))?.ToList()
              ?? new List<AiSourceEntity>();
        var drafts = messageIds.Count == 0
            ? new List<AiDraftEntity>()
            : (await draftRepository.QueryAsync(
                entity =>
                    entity.ConversationId == request.ConversationId &&
                    messageIds.Contains(entity.MessageId) &&
                    entity.TenantId == TenantContextHolder.TenantId &&
                    entity.UserId == TenantContextHolder.UserId &&
                    !entity.IsDelete,
                orderBy: OrderByConditionBuilder<AiDraftEntity>.Build(OrderByType.Asc, entity => entity.Id)))?.ToList()
              ?? new List<AiDraftEntity>();

        return new PageQueryResult<AiMessageDto>
        {
            Total = page.Total,
            List = messages.Select(message => ToMessageDto(
                message,
                sources.Where(source => source.MessageId == message.Id),
                drafts.Where(draft => draft.MessageId == message.Id))).ToList()
        };
    }

    /// <inheritdoc />
    public async Task<AiConversationEntity> GetOwnedAsync(long id)
    {
        var entity = await conversationRepository.GetAsync(conversation =>
            conversation.Id == id &&
            conversation.TenantId == TenantContextHolder.TenantId &&
            conversation.UserId == TenantContextHolder.UserId &&
            !conversation.IsDelete);
        if (entity == null)
        {
            throw new ExplicitException("会话不存在或无权访问");
        }
        return entity;
    }

    private static AiConversationDto ToConversationDto(AiConversationEntity entity)
    {
        return new AiConversationDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            UserId = entity.UserId,
            Title = entity.Title,
            Summary = entity.Summary,
            CreateUserId = entity.CreateUserId,
            CreateTime = entity.CreateTime,
            UpdateUserId = entity.UpdateUserId,
            UpdateTime = entity.UpdateTime
        };
    }

    private static AiMessageDto ToMessageDto(
        AiMessageEntity entity,
        IEnumerable<AiSourceEntity> sources,
        IEnumerable<AiDraftEntity> drafts)
    {
        return new AiMessageDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            UserId = entity.UserId,
            ConversationId = entity.ConversationId,
            Sequence = entity.Sequence,
            Role = entity.Role,
            Status = entity.Status,
            Content = entity.Content,
            Model = entity.Model,
            ErrorType = entity.ErrorType,
            CreateUserId = entity.CreateUserId,
            CreateTime = entity.CreateTime,
            UpdateUserId = entity.UpdateUserId,
            UpdateTime = entity.UpdateTime,
            Sources = sources.Select(ToSourceDto).ToList(),
            Drafts = drafts.Select(ToDraftDto).ToList()
        };
    }

    private static AiDraftDto ToDraftDto(AiDraftEntity entity)
    {
        return new AiDraftDto
        {
            Id = entity.Id,
            ConversationId = entity.ConversationId,
            MessageId = entity.MessageId,
            DraftType = entity.DraftType,
            ContentJson = entity.ContentJson,
            ExpiresAt = entity.ExpiresAt,
            ConfirmationStartedAt = entity.ConfirmationStartedAt,
            ConfirmedTargetId = entity.ConfirmedTargetId,
            Status = entity.Status == AiDraftStatus.Pending && entity.ExpiresAt <= DateTime.Now
                ? AiDraftStatus.Expired
                : entity.Status
        };
    }

    private static AiSourceDto ToSourceDto(AiSourceEntity entity)
    {
        return new AiSourceDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            UserId = entity.UserId,
            ConversationId = entity.ConversationId,
            MessageId = entity.MessageId,
            SourceType = entity.SourceType,
            SourceId = entity.SourceId,
            Title = entity.Title,
            Excerpt = entity.Excerpt,
            Date = entity.SourceDate,
            Route = entity.Route ?? entity.SourceType switch
            {
                AiSourceType.Note => $"/user/note?openNoteId={entity.SourceId}",
                AiSourceType.Todo => "/user/todoList",
                AiSourceType.DayReport => "/user/dayWorkReport",
                AiSourceType.WeekReport => "/user/weekWorkReport",
                AiSourceType.MonthReport => "/user/monthWorkReport",
                AiSourceType.Notification => $"/user/message?openMessageId={entity.SourceId}",
                AiSourceType.Menu when entity.Excerpt?.StartsWith('/') == true => entity.Excerpt,
                _ => null
            },
            CreateUserId = entity.CreateUserId,
            CreateTime = entity.CreateTime,
            UpdateUserId = entity.UpdateUserId,
            UpdateTime = entity.UpdateTime
        };
    }
}
