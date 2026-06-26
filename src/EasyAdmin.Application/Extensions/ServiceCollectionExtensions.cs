using System.Reflection;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Extensions;
using EasyAdmin.Infrastructure.Extensions;
using EasyAdmin.Infrastructure.Models;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Sean.Utility.Extensions;

namespace EasyAdmin.Application.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 应用层依赖注入
    /// </summary>
    /// <param name="services"></param>
    public static void AddApplicationDI(this IServiceCollection services)
    {
        services.AddDomainDI();

        MapsterProfile.RegisterMaps();
        services.AddMapster();

        services.AddServiceByInterfaceSuffix(Assembly.GetExecutingAssembly(), "Service", ServiceLifetime.Transient);

        services.AddScoped<IExportService, ExportService>();
        services.AddSingleton<INotePdfRenderer, WkHtmlNotePdfRenderer>();
        services.Configure<EmailOptions>(services.GetConfiguration().GetSection("Email"));
        services.Configure<SmsOptions>(services.GetConfiguration().GetSection("Sms"));
        services.AddTransient<INotificationChannelDispatcher, NotificationChannelDispatcher>();
        services.AddTransient<IEmailSender, SmtpEmailSender>();
        services.AddTransient<ISmsSender, LoggingSmsSender>();
    }
}
