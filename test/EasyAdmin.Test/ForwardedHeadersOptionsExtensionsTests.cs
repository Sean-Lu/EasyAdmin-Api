using EasyAdmin.Web.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;

namespace EasyAdmin.Test;

[TestClass]
public class ForwardedHeadersOptionsExtensionsTests
{
    [TestMethod]
    public async Task ConfigureForLocalProxy_ShouldUseForwardedIpFromLoopbackProxy()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Loopback;
        context.Request.Headers["X-Forwarded-For"] = "192.168.6.25";
        var options = new ForwardedHeadersOptions().ConfigureForLocalProxy();
        var middleware = new ForwardedHeadersMiddleware(
            _ => Task.CompletedTask,
            NullLoggerFactory.Instance,
            Options.Create(options));

        await middleware.Invoke(context);

        Assert.AreEqual(IPAddress.Parse("192.168.6.25"), context.Connection.RemoteIpAddress);
    }

    [TestMethod]
    public async Task ConfigureForLocalProxy_ShouldIgnoreForwardedIpFromUnknownProxy()
    {
        var proxyIp = IPAddress.Parse("203.0.113.10");
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = proxyIp;
        context.Request.Headers["X-Forwarded-For"] = "192.168.6.25";
        var options = new ForwardedHeadersOptions().ConfigureForLocalProxy();
        var middleware = new ForwardedHeadersMiddleware(
            _ => Task.CompletedTask,
            NullLoggerFactory.Instance,
            Options.Create(options));

        await middleware.Invoke(context);

        Assert.AreEqual(proxyIp, context.Connection.RemoteIpAddress);
    }
}
