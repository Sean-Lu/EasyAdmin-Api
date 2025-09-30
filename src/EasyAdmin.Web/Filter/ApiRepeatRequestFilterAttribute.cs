using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sean.Core.Redis;
using Sean.Utility.Security.Provider;

namespace EasyAdmin.Web.Filter;

/// <summary>
/// 接口重复请求过滤器
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
public class ApiRepeatRequestFilterAttribute : ActionFilterAttribute
{
    private readonly string[]? _requestKeys;
    private string _cacheKey;

    public ApiRepeatRequestFilterAttribute(params string[]? requestKeys)
    {
        _requestKeys = requestKeys;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        //var controllerName = context.ActionDescriptor.RouteValues["controller"];
        //var actionName = context.ActionDescriptor.RouteValues["action"];
        //var userName = context.HttpContext.User.Identity?.Name;// 当使用JWT进行身份验证时，User.Identity.Name通常不会自动返回用户名
        //var key = string.Concat(controllerName, "_", actionName, "_", userName);

        var requestPath = context.HttpContext.Request.Path;
        var userId = context.HttpContext.User.Claims.FirstOrDefault(t => t.Type == nameof(JwtUserModel.UserId))?.Value;
        var key = string.Concat(requestPath, "_", userId);
        var mdArr = new List<string> { key };
        if (_requestKeys != null && _requestKeys.Length > 0)
        {
            foreach (var rkey in _requestKeys)
            {
                if (string.IsNullOrWhiteSpace(rkey))
                {
                    continue;
                }
                string? str = context.HttpContext.Request.Query[rkey];
                if (string.IsNullOrWhiteSpace(str))
                    continue;
                mdArr.Add(str.ToLower());
            }
        }
        var md = new HashCryptoProvider().MD5(string.Join("|", mdArr));
        _cacheKey = string.Concat("repeat_request_limit_", md);

        var cacheValue = RedisHelper.StringIncrement(_cacheKey);
        if (cacheValue == 1)
        {
            RedisHelper.KeyExpire(_cacheKey, TimeSpan.FromMinutes(3));
            return;
        }

        context.Result = new JsonResult(ApiResult.Fail("请勿重复提交"));
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        RedisHelper.KeyDelete(_cacheKey);
    }
}