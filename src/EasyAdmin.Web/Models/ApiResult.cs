namespace EasyAdmin.Web.Models;

public class ApiResult
{
    public bool Success { get; set; }
    public int Code { get; set; }
    public string Msg { get; set; }

    public static ApiResult Ok()
    {
        return new ApiResult
        {
            Success = true,
            Code = 200,
            Msg = "success"
        };
    }
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

    public static ApiResult Fail()
    {
        return Fail("系统异常");
    }
    public static ApiResult Fail(string msg)
    {
        return new ApiResult
        {
            Success = false,
            Code = 500,
            Msg = msg
        };
    }
    public static ApiResult<T> Fail<T>()
    {
        return Fail<T>("系统异常");
    }
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

public class ApiResult<T> : ApiResult
{
    public T Data { get; set; }
}