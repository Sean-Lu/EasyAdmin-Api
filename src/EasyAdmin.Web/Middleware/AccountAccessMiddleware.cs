using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Web.Contracts;

namespace EasyAdmin.Web.Middleware;

public class AccountAccessMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IAccountAccessService accountAccessService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst(nameof(JwtUserModel.TenantId));
            var userClaim = context.User.FindFirst(nameof(JwtUserModel.UserId));
            if (tenantClaim == null || userClaim == null ||
                !long.TryParse(tenantClaim.Value, out var tenantId) ||
                !long.TryParse(userClaim.Value, out var userId) ||
                !await accountAccessService.IsAllowedAsync(tenantId, userId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }

        await next(context);
    }
}
