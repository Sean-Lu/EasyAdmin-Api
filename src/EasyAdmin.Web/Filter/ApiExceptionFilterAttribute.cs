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
public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    public ApiExceptionFilterAttribute()
    {

    }

    public override void OnException(ExceptionContext context)
    {
        context.Result = new JsonResult(ApiResult.Fail(context.Exception is ExplicitException ? context.Exception.Message : $"系统异常：{context.Exception.Message}"));
        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
        context.ExceptionHandled = true;
    }
}