namespace EasyAdmin.Web.Extensions
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// 获取客户端的 IP 地址
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string? GetClientIp(this HttpContext httpContext)
        {
            return httpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}