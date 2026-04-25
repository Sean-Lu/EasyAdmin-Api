using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 月报
/// </summary>
public class MonthWorkReportController(
    ILogger<MonthWorkReportController> logger,
    IMapper mapper,
    IMonthWorkReportService monthWorkReportService,
    IExportService exportService
    ) : BaseApiController
{
    /// <summary>
    /// 新增月报
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(MonthWorkReportDto data)
    {
        return Success(await monthWorkReportService.AddAsync(data));
    }

    /// <summary>
    /// 删除月报
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
            return Success(await monthWorkReportService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await monthWorkReportService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改月报
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(MonthWorkReportDto data)
    {
        return Success(await monthWorkReportService.UpdateAsync(data));
    }

    /// <summary>
    /// 分页查询月报列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<MonthWorkReportDto>>> Page([FromQuery] MonthWorkReportPageReqDto request)
    {
        var pageResult = await monthWorkReportService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<MonthWorkReportDto>>(pageResult));
    }

    /// <summary>
    /// 查询月报详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<MonthWorkReportDto>> Detail(long id)
    {
        return Success(mapper.Map<MonthWorkReportDto>(await monthWorkReportService.GetByIdAsync(id)));
    }

    /// <summary>
    /// 导出
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Export([FromBody] MonthWorkReportExportReqDto request)
    {
        if (request.ExportAll)
        {
            request.PageNumber = 1;
            request.PageSize = int.MaxValue;
        }

        var list = (await monthWorkReportService.PageAsync(request)).List;
        var exportDtos = mapper.Map<List<MonthWorkReportExportDto>>(list);

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

        var fileName = $"月报_{DateTime.Now:yyyyMMdd}.{fileExtension}";
        return File(fileBytes, contentType, fileName);
    }
}
