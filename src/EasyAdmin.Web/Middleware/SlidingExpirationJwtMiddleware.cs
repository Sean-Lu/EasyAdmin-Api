//namespace EasyAdmin.Web.Middleware;

//public class SlidingExpirationJwtMiddleware
//{
//    private readonly RequestDelegate _next;
//    private readonly IConfiguration _configuration;
//    private readonly IJwtTokenService _jwtTokenService; // 假设你有一个服务来生成JWT令牌

//    public SlidingExpirationJwtMiddleware(RequestDelegate next, IConfiguration configuration, IJwtTokenService jwtTokenService)
//    {
//        _next = next;
//        _configuration = configuration;
//        _jwtTokenService = jwtTokenService;
//    }

//    public async Task InvokeAsync(HttpContext context)
//    {
//        // 检查请求头中是否有Authorization，并提取JWT
//        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
//        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
//        {
//            var token = authHeader.Substring("Bearer ".Length).Trim();

//            // 验证JWT有效性
//            var isValid = await _jwtTokenService.ValidateTokenAsync(token);
//            if (isValid)
//            {
//                // 检查是否接近过期时间
//                var tokenExpiration = _jwtTokenService.GetExpirationTime(token);
//                var now = DateTime.UtcNow;
//                var isNearExpiration = tokenExpiration - now < TimeSpan.FromMinutes(5);

//                if (isNearExpiration)
//                {
//                    // 生成新的JWT令牌
//                    var newToken = await _jwtTokenService.GenerateTokenAsync(user); // 假设user是已认证的用户对象

//                    // 更新响应头中的Authorization
//                    context.Response.Headers["Authorization"] = $"Bearer {newToken}";
//                }
//            }
//        }

//        // 调用下一个中间件
//        await _next(context);
//    }
//}