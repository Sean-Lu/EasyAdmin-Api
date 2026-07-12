using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 服务器监控服务
/// </summary>
public interface IServerMonitorService
{
    Task<ServerMonitorOverviewDto> GetOverviewAsync();
}
