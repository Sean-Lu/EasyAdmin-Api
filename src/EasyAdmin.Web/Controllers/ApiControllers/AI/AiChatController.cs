using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sean.Core.DbRepository;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// AI对话
/// </summary>
public sealed class AiChatController(
    IAiConversationService conversationService,
    IAiChatService aiChatService,
    IAiToolService aiToolService,
    IOptions<MvcNewtonsoftJsonOptions> jsonOptions) : BaseApiController
{
    /// <summary>
    /// 新建会话
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<AiConversationDto>> CreateConversation()
    {
        return Success(await conversationService.CreateAsync());
    }

    /// <summary>
    /// 分页查询会话
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<PageQueryResult<AiConversationDto>>> ConversationPage(AiConversationPageReqDto request)
    {
        return Success(await conversationService.PageAsync(request));
    }

    /// <summary>
    /// 重命名会话
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<AiConversationDto>> RenameConversation(AiConversationRenameReqDto request)
    {
        return Success(await conversationService.RenameAsync(request.Id, request.Title));
    }

    /// <summary>
    /// 删除会话
    /// </summary>
    [HttpPost]
    public async Task<ApiResult> DeleteConversation(AiConversationDeleteReqDto request)
    {
        await conversationService.DeleteAsync(request.Id);
        return Success();
    }

    /// <summary>
    /// 分页查询消息
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<PageQueryResult<AiMessageDto>>> MessagePage(AiMessagePageReqDto request)
    {
        return Success(await conversationService.PageMessagesAsync(request));
    }

    /// <summary>
    /// 流式生成回复
    /// </summary>
    [HttpPost]
    public async Task Stream(AiChatRequestDto request)
    {
        var cancellationToken = HttpContext.RequestAborted;
        await using var enumerator = aiChatService
            .StreamAsync(request, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);

        // 首次推进发生在响应头写出前，使校验、权限、配置和配额异常沿用统一异常响应
        if (!await enumerator.MoveNextAsync())
        {
            return;
        }

        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no";

        try
        {
            await WriteEventAsync(enumerator.Current, cancellationToken);
            while (await enumerator.MoveNextAsync())
            {
                await WriteEventAsync(enumerator.Current, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                await WriteEventAsync(
                    new AiStreamEventDto
                    {
                        Type = "error",
                        Data = new { message = "AI服务暂时不可用，请稍后重试" }
                    },
                    cancellationToken);
            }
        }
    }

    /// <summary>
    /// 取消生成
    /// </summary>
    [HttpPost]
    public async Task<ApiResult> Cancel(AiChatCancelReqDto request)
    {
        await aiChatService.CancelAsync(request.ConversationId);
        return Success();
    }

    /// <summary>
    /// 查询当前租户AI可用状态
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<bool>> Availability()
    {
        return Success(await aiChatService.IsAvailableAsync());
    }

    /// <summary>
    /// 解析可访问来源
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<AiSourceResolveDto>> ResolveSource(AiSourceResolveReqDto request)
    {
        return Success(new AiSourceResolveDto
        {
            Route = await aiToolService.ResolveSourceAsync(request.SourceType, request.SourceId)
        });
    }

    private async Task WriteEventAsync(
        AiStreamEventDto item,
        CancellationToken cancellationToken)
    {
        await Response.WriteAsync($"event: {item.Type}\n", cancellationToken);
        await Response.WriteAsync(
            $"data: {JsonConvert.SerializeObject(item.Data, jsonOptions.Value.SerializerSettings)}\n\n",
            cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
}
