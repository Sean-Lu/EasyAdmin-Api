using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using EasyAdmin.Infrastructure.Storage;

namespace EasyAdmin.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceByInterfaceSuffix(this IServiceCollection services, Assembly assembly, string interfaceSuffix, ServiceLifetime serviceLifetime)
    {
        var types = assembly.GetTypes();
        var interfaceTypes = types.Where(c => c.IsInterface && c.Name.EndsWith(interfaceSuffix));
        foreach (var interfaceType in interfaceTypes)
        {
            var implType = types.FirstOrDefault(c => c.Name == interfaceType.Name[1..]);
            if (implType != null)
            {
                switch (serviceLifetime)
                {
                    case ServiceLifetime.Singleton:
                        services.AddSingleton(interfaceType, implType);
                        break;
                    case ServiceLifetime.Transient:
                        services.AddTransient(interfaceType, implType);
                        break;
                    case ServiceLifetime.Scoped:
                        services.AddScoped(interfaceType, implType);
                        break;
                }
            }
        }
        return services;
    }
    public static IServiceCollection AddServiceByClassSuffix(this IServiceCollection services, Assembly assembly, string classSuffix, ServiceLifetime serviceLifetime)
    {
        var serviceImplList = assembly.GetTypes().Where(s => s is { IsClass: true, IsInterface: false, IsAbstract: false } && s.Name.EndsWith(classSuffix)).ToList();

        var result = new Dictionary<Type, Type[]>();
        foreach (var serviceImpl in serviceImplList)
        {
            var interfaceTypeArray = serviceImpl.GetInterfaces().Where(t => t.Name.EndsWith(classSuffix)).ToArray();
            result.Add(serviceImpl, interfaceTypeArray);
        }

        foreach (var implementationType in result)
        {
            foreach (var serviceType in implementationType.Value)
            {
                switch (serviceLifetime)
                {
                    case ServiceLifetime.Singleton:
                        services.AddSingleton(serviceType, implementationType.Key);
                        break;
                    case ServiceLifetime.Transient:
                        services.AddTransient(serviceType, implementationType.Key);
                        break;
                    case ServiceLifetime.Scoped:
                        services.AddScoped(serviceType, implementationType.Key);
                        break;
                }
            }
        }

        return services;
    }

    /// <summary>
    /// 添加文件存储服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddFileStorage(this IServiceCollection services)
    {
        services.AddSingleton<IFileStorage, LocalFileStorage>();// 添加本地文件存储服务
        services.AddSingleton<IFileStorage, AliyunOssStorage>();// 添加阿里云OSS文件存储服务
        services.AddSingleton<IFileStorageFactory, FileStorageFactory>();// 添加文件存储工厂服务
        return services;
    }
}