using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// AI草稿服务接口
/// </summary>
public interface IAiDraftService
{
    /// <summary>
    /// 获取草稿
    /// </summary>
    Task<AiDraftDto> GetAsync(long id);

    /// <summary>
    /// 更新草稿
    /// </summary>
    Task<AiDraftDto> UpdateAsync(AiDraftUpdateDto request);

    /// <summary>
    /// 删除草稿
    /// </summary>
    Task DeleteAsync(long id);

    /// <summary>
    /// 确认并创建业务数据
    /// </summary>
    Task<AiDraftConfirmResultDto> ConfirmAsync(long id);

    /// <summary>
    /// 根据模型输出创建草稿
    /// </summary>
    Task<AiDraftEntity> CreateFromModelAsync(
        long conversationId,
        long messageId,
        AiDraftType type,
        string contentJson);
}