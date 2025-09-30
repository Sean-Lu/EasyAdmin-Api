using EasyAdmin.Application.Extensions;
using EasyAdmin.JobService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sean.Core.Quartz.Extensions;
using Sean.Utility.Contracts;
using Sean.Utility.Impls.Log;

Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    }).ConfigureServices(services =>
    {
        services.AddHostedService<MainService>();
        services.AddTransient(typeof(ISimpleLogger<>), typeof(SimpleLocalLogger<>));
        services.AddQuartzJobs(ServiceLifetime.Scoped);
        services.AddApplicationDI();
    }).UseWindowsService()//部署Windows服务
    .UseSystemd()//部署Linux服务
    .UseConsoleLifetime()
    .Build()
    .Run();
