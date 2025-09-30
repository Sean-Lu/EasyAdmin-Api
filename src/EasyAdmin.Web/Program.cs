using EasyAdmin.Application.Extensions;
using EasyAdmin.Web.Extensions;
using EasyAdmin.Web.Helper;
using EasyAdmin.Web.Middleware;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;
using Nacos.V2.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Sean.Core.Redis.Extensions;
using System.Reflection;
using EasyAdmin.Infrastructure.Converter;
using EasyAdmin.Web.Filter;
using OfficeOpenXml;

Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);// 设置当前工作目录：@".\"

ThreadPool.SetMinThreads(30, 30);

ExcelPackage.License.SetNonCommercialPersonal("Sean");// EPPlus

var builder = WebApplication.CreateBuilder(args);

//Console.WriteLine("环境变量 ASPNETCORE_URLS: " + Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));// 控制台输出环境变量

//builder.Configuration.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
//builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
//builder.Configuration.AddEnvironmentVariables();
//if (builder.Environment.IsDevelopment())
//{
//    builder.Configuration.AddUserSecrets<Program>();
//}

const string nacosConfigSectionName = "NacosConfig";
var enableNacos = builder.Configuration.GetValue<bool>($"{nacosConfigSectionName}:Enable");
if (enableNacos)
{
    builder.Host.UseNacosConfig(section: nacosConfigSectionName, parser: null, logAction: null);// Nacos 配置中心
}

// 打印配置
//Console.WriteLine("配置 ASPNETCORE_URLS: " + builder.Configuration.GetValue<string>("ASPNETCORE_URLS"));

#region [ConfigureServices] Add services to the container.
//builder.Services.AddControllersWithViews();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<TenantFilter>();
}).AddNewtonsoftJson(options =>
{
    // Newtonsoft.Json
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    options.SerializerSettings.Converters.Add(new JsonLongToStringConverter());
    //options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;// 循环引用
})/*.AddJsonOptions(options =>
{
    // System.Text.Json
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new SystemTextJsonLongToStringConverter());
})*/;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "EasyAdmin.Web",
        Description = "每一次的努力，都会让你离成功更近一步。请继续前行，风雨无阻，你会收获阳光。"
    });
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"), true);
    JwtHelper.SwaggerAddJwt(options);
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var jwtConfig = new JwtConfig();
builder.Configuration.Bind("Jwt", jwtConfig);
builder.Services.AddJwtService(jwtConfig);

builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
    options.Limits.MinRequestBodyDataRate = null;
    options.Limits.MinResponseDataRate = null;
});

//var configuration = builder.Services.GetConfiguration();

if (enableNacos)
{
    builder.Services.AddNacosV2Config(builder.Configuration, sectionName: nacosConfigSectionName);// Nacos 配置中心：INacosConfigService
    //builder.Services.AddNacosAspNet(builder.Configuration, "nacos");// Nacos 服务发现
}

builder.Services.AddRedis(builder.Configuration);

builder.Services.AddApplicationDI();
#endregion

var app = builder.Build();

//// 读取Nacos配置并添加到IConfiguration
//var nacosConfig = app.Services.GetRequiredService<INacosConfigService>();
//var config = await nacosConfig.GetConfig("common", "DEFAULT_GROUP", 3000);
//builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(config)));

#region Configure the HTTP request pipeline.
app.UseCors("AllowAll");

app.UseMiddleware<GlobalExceptionMiddleware>();// 全局异常捕获中间件
// app.UseMiddleware<SlidingExpirationJwtMiddleware>();// JWT滑动过期中间件：需要在客户端（例如JavaScript）中处理返回的新令牌，并在后续的请求中使用它。这通常涉及到监听响应头中的变化，并在需要时更新存储在客户端的令牌。

//// 解决Nginx代理不能获取IP问题
//app.UseForwardedHeaders(new ForwardedHeadersOptions
//{
//    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
//});

//if (app.Environment.IsDevelopment())
{
    //app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();//要在授权之前认证，这个和[Authorize]特性有关
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();
#endregion

app.Run();
