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

Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);// ���õ�ǰ����Ŀ¼��@".\"

ThreadPool.SetMinThreads(30, 30);

ExcelPackage.License.SetNonCommercialPersonal("Sean");// EPPlus

var builder = WebApplication.CreateBuilder(args);

//Console.WriteLine("�������� ASPNETCORE_URLS: " + Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));// ����̨�����������

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
    builder.Host.UseNacosConfig(section: nacosConfigSectionName, parser: null, logAction: null);// Nacos ��������
}

// ��ӡ����
//Console.WriteLine("���� ASPNETCORE_URLS: " + builder.Configuration.GetValue<string>("ASPNETCORE_URLS"));

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
    //options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;// ѭ������
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
        Description = "ÿһ�ε�Ŭ��������������ɹ�����һ���������ǰ�У��������裬����ջ����⡣"
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
    builder.Services.AddNacosV2Config(builder.Configuration, sectionName: nacosConfigSectionName);// Nacos �������ģ�INacosConfigService
    //builder.Services.AddNacosAspNet(builder.Configuration, "nacos");// Nacos ������
}

builder.Services.AddRedis(builder.Configuration);

builder.Services.AddApplicationDI();
#endregion

var app = builder.Build();

//// ��ȡNacos���ò���ӵ�IConfiguration
//var nacosConfig = app.Services.GetRequiredService<INacosConfigService>();
//var config = await nacosConfig.GetConfig("common", "DEFAULT_GROUP", 3000);
//builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(config)));

#region Configure the HTTP request pipeline.
app.UseCors("AllowAll");

app.UseMiddleware<GlobalExceptionMiddleware>();// ȫ���쳣�����м��
// app.UseMiddleware<SlidingExpirationJwtMiddleware>();// JWT���������м������Ҫ�ڿͻ��ˣ�����JavaScript���д����ص������ƣ����ں�����������ʹ��������ͨ���漰��������Ӧͷ�еı仯��������Ҫʱ���´洢�ڿͻ��˵����ơ�

//// ���Nginx�����ܻ�ȡIP����
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

app.UseAuthentication();//Ҫ����Ȩ֮ǰ��֤�������[Authorize]�����й�
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();
#endregion

app.Run();
