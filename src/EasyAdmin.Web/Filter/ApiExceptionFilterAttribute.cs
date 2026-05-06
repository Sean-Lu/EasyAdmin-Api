using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Web.Filter;

/// <summary>
/// 接口异常过滤器
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
public class ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> logger) : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        var requestPath = context.HttpContext.Request.Path;
        var requestMethod = context.HttpContext.Request.Method;
        var exceptionType = context.Exception.GetType().Name;
        var exceptionMessage = context.Exception.Message;
        var stackTrace = context.Exception.StackTrace;

        if (context.Exception is ExplicitException)
        {
            logger.LogWarning("业务异常 - 请求路径: {RequestPath}, 请求方法: {RequestMethod}, 异常类型: {ExceptionType}, 异常信息: {ExceptionMessage}",
                requestPath, requestMethod, exceptionType, exceptionMessage);
        }
        else
        {
            logger.LogError(context.Exception, "系统异常 - 请求路径: {RequestPath}, 请求方法: {RequestMethod}, 异常类型: {ExceptionType}, 异常信息: {ExceptionMessage}",
                requestPath, requestMethod, exceptionType, exceptionMessage);
        }

        context.Result = new JsonResult(ApiResult.Fail(context.Exception is ExplicitException ? exceptionMessage : $"系统异常：{exceptionMessage}"));
        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
        context.ExceptionHandled = true;
    }
}