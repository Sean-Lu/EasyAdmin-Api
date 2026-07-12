using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 服务器监控
/// </summary>
public class ServerMonitorController(IServerMonitorService serverMonitorService) : BaseApiController
{
    /// <summary>
    /// 获取服务器监控快照
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ServerMonitorOverviewDto>> Overview()
    {
        return Success(await serverMonitorService.GetOverviewAsync());
    }
}
