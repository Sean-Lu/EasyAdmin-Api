using EasyAdmin.Application.Extensions;
using EasyAdmin.ConsumerService;
using EasyAdmin.ConsumerService.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        services.AddConsumers();
        services.AddApplicationDI();
    }).UseConsoleLifetime().Build().Run();
