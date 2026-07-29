using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// AI聊天服务接口
/// </summary>
public interface IAiChatService
{
    /// <summary>
    /// 流式生成回复
    /// </summary>
    IAsyncEnumerable<AiStreamEventDto> StreamAsync(
        AiChatRequestDto request,
        CancellationToken cancellationToken);

    /// <summary>
    /// 取消会话生成
    /// </summary>
    Task CancelAsync(long conversationId);

    /// <summary>
    /// 判断AI功能是否可用
    /// </summary>
    Task<bool> IsAvailableAsync();
}