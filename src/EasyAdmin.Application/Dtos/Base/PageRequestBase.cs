namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 分页请求基础类
/// </summary>
public class PageRequestBase
{
    /// <summary>
    /// 当前页（从1开始）
    /// </summary>
    public int PageNumber { get; set; } = 1;
    /// <summary>
    /// 每页的数量
    /// </summary>
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// API结果分页数据类
/// </summary>
/// <typeparam name="T">分页数据项类型</typeparam>
public class ApiResultPageData<T> : PageRequestBase
{
    public int Total { get; set; }
    public List<T> List { get; set; }
}