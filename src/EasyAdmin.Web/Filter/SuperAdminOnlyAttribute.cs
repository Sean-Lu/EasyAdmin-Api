using EasyAdmin.Domain.Contracts;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EasyAdmin.Web.Filter;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SuperAdminOnlyAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var tenantClaim = context.HttpContext.User.FindFirst(nameof(JwtUserModel.TenantId));
        var userClaim = context.HttpContext.User.FindFirst(nameof(JwtUserModel.UserId));
        if (tenantClaim?.Value != SysConst.DefaultTenantId.ToString() || !long.TryParse(userClaim?.Value, out var userId))
        {
            context.Result = new ForbidResult();
            return;
        }

        var repository = context.HttpContext.RequestServices.GetRequiredService<IUserRoleRepository>();
        var allowed = await repository.ExistsAsync(entity =>
            entity.TenantId == SysConst.DefaultTenantId && entity.UserId == userId &&
            entity.RoleId == SysConst.SuperAdminRoleId && !entity.IsDelete);
        if (!allowed)
            context.Result = new ForbidResult();
    }
}
