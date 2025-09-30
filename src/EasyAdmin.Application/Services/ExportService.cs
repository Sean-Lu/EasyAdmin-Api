using System.ComponentModel;
using System.Reflection;
using EasyAdmin.Application.Contracts;
using System.Text;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Attributes;
using OfficeOpenXml;

namespace EasyAdmin.Application.Services;

public class ExportService : IExportService
{
    public async Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Sheet1");

        // 使用反射获取属性名作为列标题
        var properties = typeof(T).GetProperties();
        for (int i = 0; i < properties.Length; i++)
        {
            var propertyInfo = properties[i];
            var description = propertyInfo.GetCustomAttribute<DescriptionAttribute>(true)?.Description ?? propertyInfo.Name;
            worksheet.Cells[1, i + 1].Value = description;
        }

        // 填充数据
        int row = 2;
        foreach (var item in data)
        {
            for (int i = 0; i < properties.Length; i++)
            {
                var propertyInfo = properties[i];
                worksheet.Cells[row, i + 1].Value = GetConvertValue(propertyInfo, item);
            }
            row++;
        }

        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> ExportToMarkdownAsync<T>(IEnumerable<T> data)
    {
        var sb = new StringBuilder();
        var properties = typeof(T).GetProperties();

        // 添加表头
        sb.Append("| ");
        foreach (var prop in properties)
        {
            var description = prop.GetCustomAttribute<DescriptionAttribute>(true)?.Description ?? prop.Name;
            sb.Append(description).Append(" | ");
        }
        sb.AppendLine();

        // 添加分隔线
        sb.Append("| ");
        foreach (var _ in properties)
        {
            sb.Append("---").Append(" | ");
        }
        sb.AppendLine();

        // 添加数据行
        foreach (var item in data)
        {
            sb.Append("| ");
            foreach (var prop in properties)
            {
                sb.Append(GetConvertValue(prop, item)).Append(" | ");
            }
            sb.AppendLine();
        }

        return await Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    public async Task<byte[]> ExportToMarkdownAsync(List<DayWorkReportExportDto>? data)
    {
        if (data == null || !data.Any())
        {
            return await Task.FromResult(Encoding.UTF8.GetBytes("没有数据"));
        }

        var sb = new StringBuilder();

        foreach (var dto in data)
        {
            sb.AppendLine($"### {dto.RecordTime.ToString("yyyy-MM-dd")}");
            sb.AppendLine();
            sb.AppendLine("今日工作：");
            sb.AppendLine();
            sb.AppendLine(dto.TodayWork);
            sb.AppendLine();
            sb.AppendLine("明日计划：");
            sb.AppendLine();
            sb.AppendLine(dto.TomorrowPlan);
            sb.AppendLine();
        }

        return await Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private string GetConvertValue(PropertyInfo propertyInfo, object? obj)
    {
        var value = propertyInfo.GetValue(obj);
        string convertValue;
        if (value is DateTime dt)
        {
            var dateTimeFormat = propertyInfo.GetCustomAttribute<DateTimeFormatAttribute>(true)?.Format ?? "yyyy-MM-dd HH:mm:ss";
            convertValue = dt.ToString(dateTimeFormat);
        }
        else
        {
            convertValue = value?.ToString() ?? string.Empty;
        }
        return convertValue;
    }
}