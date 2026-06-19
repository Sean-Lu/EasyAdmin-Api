namespace EasyAdmin.Web.Models;

/// <summary>
/// API结果模型
/// </summary>
public class ApiResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    /// <summary>
    /// 状态码
    /// </summary>
    public int Code { get; set; }
    /// <summary>
    /// 消息
    /// </summary>
    public string Msg { get; set; }

    /// <summary>
    /// 成功
    /// </summary>
    public static ApiResult Ok()
    {
        return new ApiResult
        {
            Success = true,
            Code = 200,
            Msg = "success"
        };
    }
    /// <summary>
    /// 成功
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="data">数据</param>
    /// <returns>包含结果的API结果</returns>
    public static ApiResult<T> Ok<T>(T data)
    {
        return new ApiResult<T>
        {
            Success = true,
            Code = 200,
            Msg = "success",
            Data = data
        };
    }

    /// <summary>
    /// 失败
    /// </summary>
    public static ApiResult Fail()
    {
        return Fail("系统异常");
    }
    /// <summary>
    /// 失败
    /// </summary>
    /// <param name="msg">消息</param>
    /// <returns>包含结果的API结果</returns>
    public static ApiResult Fail(string msg)
    {
        return new ApiResult
        {
            Success = false,
            Code = 500,
            Msg = msg
        };
    }
    /// <summary>
    /// 失败
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <returns>包含结果的API结果</returns>
    public static ApiResult<T> Fail<T>()
    {
        return Fail<T>("系统异常");
    }
    /// <summary>
    /// 失败
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="msg">消息</param>
    /// <returns>包含结果的API结果</returns>
    public static ApiResult<T> Fail<T>(string msg)
    {
        return new ApiResult<T>
        {
            Success = false,
            Code = 500,
            Msg = msg
        };
    }
}

/// <summary>
/// API结果模型（包含数据）
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class ApiResult<T> : ApiResult
{
    /// <summary>
    /// 数据
    /// </summary>
    public T? Data { get; set; }
}