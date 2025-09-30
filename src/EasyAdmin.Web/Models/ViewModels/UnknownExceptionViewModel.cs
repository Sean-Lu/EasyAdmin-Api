namespace EasyAdmin.Web.Models.ViewModels;

/// <summary>
/// 未知异常错误页面实体模型
/// </summary>
public class UnknownExceptionViewModel
{
    /// <summary>
    /// 请求url地址
    /// </summary>
    public string RequestUrl { get; set; }
    /// <summary>
    /// 异常信息
    /// </summary>
    public Exception Exception { get; set; }
    /// <summary>
    /// 是否在页面显示异常信息
    /// </summary>
    public bool ShowException { get; set; }
}