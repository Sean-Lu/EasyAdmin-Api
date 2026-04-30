using System.Reflection;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Extensions;
using EasyAdmin.Infrastructure.Extensions;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

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
    }
}