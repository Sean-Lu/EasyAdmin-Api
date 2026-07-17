using EasyAdmin.Application.Dtos;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 收藏服务
/// </summary>
public interface IFavoriteService
{
    Task<PageQueryResult<FavoriteListItemDto>> PageAsync(FavoritePageReqDto request);
    Task<FavoriteMutationResultDto> AddAsync(FavoriteTargetReqDto request);
    Task<FavoriteMutationResultDto> AddShareAsync(FavoriteAddShareReqDto request);
    Task<FavoriteMutationResultDto> DeleteAsync(FavoriteDeleteReqDto request);
    Task<List<FavoriteStatusItemDto>> GetStatusAsync(FavoriteStatusReqDto request);
}
