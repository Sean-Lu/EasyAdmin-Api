namespace EasyAdmin.Application.Dtos;

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

public class ApiResultPageData<T> : PageRequestBase
{
    public int Total { get; set; }
    public List<T> List { get; set; }
}