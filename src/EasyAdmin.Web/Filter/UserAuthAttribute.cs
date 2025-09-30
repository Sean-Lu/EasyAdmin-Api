using Microsoft.AspNetCore.Mvc.Filters;

namespace EasyAdmin.Web.Filter;

/// <summary>
/// 用户授权校验过滤器
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
public class UserAuthAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {

    }
}