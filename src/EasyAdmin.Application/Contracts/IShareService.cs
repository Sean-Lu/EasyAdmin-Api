using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 分享服务
/// </summary>
public interface IShareService
{
    /// <summary>
    /// 获取我的分享
    /// </summary>
    Task<PageQueryResult<ShareListItemDto>> ListAsync(ShareListReqDto request);
    Task<ShareConfigDto> GetConfigAsync(ShareTargetType targetType, long targetId);
    Task<ShareConfigDto> SaveAsync(ShareSaveDto request);
    Task<ShareConfigDto> SetEnabledAsync(ShareToggleDto request);
    Task<ShareConfigDto> RegenerateAsync(ShareTargetRequestDto request);
    Task<PublicShareStatusDto> GetPublicStatusAsync(string shareCode);
    Task<PublicShareVerifyResultDto> VerifyPasswordAsync(PublicShareVerifyDto request, string ipAddress);
    Task<PublicShareFileDto> GetPublicFileAsync(string shareCode, string? accessToken);
    Task<PublicShareNoteDto> GetPublicNoteAsync(string shareCode, string? accessToken);
    Task<PublicShareStream> OpenPublicFileAsync(string shareCode, string? accessToken);
    Task<PublicShareStream> OpenPublicNoteImageAsync(string shareCode, long fileId, string? accessToken);
}