using System.Reflection;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Extensions;
using EasyAdmin.Infrastructure.Extensions;
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

        services.AddAutoMapper(expression =>
        {
            expression.AllowNullCollections = true;
            expression.AllowNullDestinationValues = true;
            expression.AddMaps(typeof(AutoMapperProfile));
        });

        services.AddServiceByInterfaceSuffix(Assembly.GetExecutingAssembly(), "Service", ServiceLifetime.Transient);

        services.AddScoped<IExportService, ExportService>();
    }
}