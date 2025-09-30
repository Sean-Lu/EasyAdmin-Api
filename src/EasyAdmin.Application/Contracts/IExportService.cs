using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

public interface IExportService
{
    /// <summary>
    /// 导出Excel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data);
    /// <summary>
    /// 导出Markdown
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    Task<byte[]> ExportToMarkdownAsync<T>(IEnumerable<T> data);

    /// <summary>
    /// 导出Markdown - 日报
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    Task<byte[]> ExportToMarkdownAsync(List<DayWorkReportExportDto>? data);
}