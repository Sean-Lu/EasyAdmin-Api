using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Sean.Core.DbRepository;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 我的收藏
/// </summary>
public class FavoriteController(IFavoriteService favoriteService) : BaseApiController
{
    /// <summary>
    /// 分页
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<PageQueryResult<FavoriteListItemDto>>> Page(FavoritePageReqDto request)
    {
        return Success(await favoriteService.PageAsync(request));
    }

    /// <summary>
    /// 收藏
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<FavoriteMutationResultDto>> Add(FavoriteTargetReqDto request)
    {
        return Success(await favoriteService.AddAsync(request));
    }

    /// <summary>
    /// 收藏分享
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<FavoriteMutationResultDto>> AddShare(FavoriteAddShareReqDto request)
    {
        return Success(await favoriteService.AddShareAsync(request));
    }

    /// <summary>
    /// 取消收藏
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<FavoriteMutationResultDto>> Delete(FavoriteDeleteReqDto request)
    {
        return Success(await favoriteService.DeleteAsync(request));
    }

    /// <summary>
    /// 收藏状态
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<List<FavoriteStatusItemDto>>> Status(FavoriteStatusReqDto request)
    {
        return Success(await favoriteService.GetStatusAsync(request));
    }
}
