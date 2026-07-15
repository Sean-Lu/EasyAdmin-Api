using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

namespace EasyAdmin.Web.Extensions;

/// <summary>
/// 转发头配置扩展
/// </summary>
public static class ForwardedHeadersOptionsExtensions
{
    /// <summary>
    /// 配置本机可信代理
    /// </summary>
    /// <param name="options">转发头配置</param>
    /// <returns>转发头配置</returns>
    public static ForwardedHeadersOptions ConfigureForLocalProxy(this ForwardedHeadersOptions options)
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 1;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
        options.KnownProxies.Add(IPAddress.Loopback);
        options.KnownProxies.Add(IPAddress.IPv6Loopback);
        return options;
    }
}
