using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// AI会话服务接口
/// </summary>
public interface IAiConversationService
{
    /// <summary>
    /// 新建会话
    /// </summary>
    Task<AiConversationDto> CreateAsync();

    /// <summary>
    /// 分页查询会话
    /// </summary>
    Task<PageQueryResult<AiConversationDto>> PageAsync(AiConversationPageReqDto request);

    /// <summary>
    /// 重命名会话
    /// </summary>
    Task<AiConversationDto> RenameAsync(long id, string title);

    /// <summary>
    /// 删除会话
    /// </summary>
    Task DeleteAsync(long id);

    /// <summary>
    /// 分页查询消息
    /// </summary>
    Task<PageQueryResult<AiMessageDto>> PageMessagesAsync(AiMessagePageReqDto request);

    /// <summary>
    /// 获取当前用户会话
    /// </summary>
    Task<AiConversationEntity> GetOwnedAsync(long id);
}