using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 周报
/// </summary>
public class WeekWorkReportController(
    ILogger<WeekWorkReportController> logger,
    IMapper mapper,
    IWeekWorkReportService weekWorkReportService,
    IExportService exportService
    ) : BaseApiController
{
    /// <summary>
    /// 新增周报
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(WeekWorkReportDto data)
    {
        return Success(await weekWorkReportService.AddAsync(data));
    }

    /// <summary>
    /// 删除周报
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var ids = data["ids"]?.Values<long>().ToList() ?? default;
        if (ids != null && ids.Any())
        {
            // 批量删除
            return Success(await weekWorkReportService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await weekWorkReportService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改周报
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(WeekWorkReportDto data)
    {
        return Success(await weekWorkReportService.UpdateAsync(data));
    }

    /// <summary>
    /// 分页查询周报列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<WeekWorkReportDto>>> Page([FromQuery] WeekWorkReportPageReqDto request)
    {
        var pageResult = await weekWorkReportService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<WeekWorkReportDto>>(pageResult));
    }

    /// <summary>
    /// 查询周报详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<WeekWorkReportDto>> Detail(long id)
    {
        return Success(mapper.Map<WeekWorkReportDto>(await weekWorkReportService.GetByIdAsync(id)));
    }

    /// <summary>
    /// 导出
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Export([FromBody] WeekWorkReportExportReqDto request)
    {
        if (request.ExportAll)
        {
            request.PageNumber = 1;
            request.PageSize = int.MaxValue;
        }

        var list = (await weekWorkReportService.PageAsync(request)).List;
        var exportDtos = mapper.Map<List<WeekWorkReportExportDto>>(list);

        byte[] fileBytes;
        string contentType;
        string fileExtension;

        switch (request.ExportType.ToLower())
        {
            case "excel":
                fileBytes = await exportService.ExportToExcelAsync(exportDtos);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileExtension = "xlsx";
                break;
            case "markdown":
                fileBytes = await exportService.ExportToMarkdownAsync(exportDtos);
                contentType = "text/markdown";
                fileExtension = "md";
                break;
            default:
                return BadRequest("不支持的导出类型");
        }

        var fileName = $"周报_{DateTime.Now:yyyyMMdd}.{fileExtension}";
        return File(fileBytes, contentType, fileName);
    }
}
