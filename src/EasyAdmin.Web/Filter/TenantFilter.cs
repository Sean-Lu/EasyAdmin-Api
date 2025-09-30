using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Web.Helper;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EasyAdmin.Web.Filter;

/// <summary>
/// 多租户过滤器
/// </summary>
public class TenantFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 从请求头中获取当前用户信息
        var curUserInfo = JwtHelper.GetUserInfo(context.HttpContext.Request);
        // 设置上下文
        TenantContextHolder.UserInfo = curUserInfo;

        try
        {
            await next();
        }
        finally
        {
            // 清理上下文
            TenantContextHolder.Clear();
        }
    }
}