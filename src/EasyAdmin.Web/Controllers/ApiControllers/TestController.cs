using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 测试
/// </summary>
[AllowAnonymous]
public class TestController(
    ILogger<TestController> logger,
    IConfiguration configuration
    ) : BaseApiController
{
    [HttpGet]
    public IEnumerable<object> GetWeatherForecast()
    {
        return Enumerable.Range(1, 5).Select(index => new
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            RedisUrl = configuration.GetValue<string>("Redis:EndPoints")
        }).ToArray();
    }
}