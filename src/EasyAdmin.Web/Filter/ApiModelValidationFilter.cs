using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using EasyAdmin.Web.Models;

namespace EasyAdmin.Web.Filter;

/// <summary>
/// API 模型验证过滤器
/// </summary>
public class ApiModelValidationFilter(ILogger<ApiModelValidationFilter> logger) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value.Errors.Any())
                .SelectMany(x => x.Value.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();
            var errorMessage = string.Join("; ", errors);
            var requestPath = context.HttpContext.Request.Path;
            var requestMethod = context.HttpContext.Request.Method;

            logger.LogWarning("模型验证失败 - 请求路径: {RequestPath}, 请求方法: {RequestMethod}, 错误信息: {ErrorMessage}",
                requestPath, requestMethod, errorMessage);

            context.Result = new JsonResult(ApiResult.Fail(errorMessage));
            context.HttpContext.Response.StatusCode = 200;
        }
    }
}