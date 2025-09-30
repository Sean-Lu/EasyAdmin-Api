using System.Reflection;
using EasyAdmin.ConsumerService.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace EasyAdmin.ConsumerService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConsumers(this IServiceCollection services)
    {
        var types = Assembly.GetExecutingAssembly().GetTypes().Where(c => c.IsClass && typeof(IBaseConsumer).IsAssignableFrom(c));
        foreach (var type in types)
        {
            services.AddSingleton(type);
        }
        return services;
    }
}