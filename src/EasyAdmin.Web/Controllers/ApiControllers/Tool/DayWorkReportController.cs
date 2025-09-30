using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 日报
/// </summary>
public class DayWorkReportController(
    ILogger<DayWorkReportController> logger,
    IMapper mapper,
    IDayWorkReportService dayWorkReportService,
    IExportService exportService
    ) : BaseApiController
{
    /// <summary>
    /// 新增日报
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(DayWorkReportDto data)
    {
        return Success(await dayWorkReportService.AddAsync(data));
    }

    /// <summary>
    /// 删除日报
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
            return Success(await dayWorkReportService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await dayWorkReportService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改日报
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(DayWorkReportDto data)
    {
        return Success(await dayWorkReportService.UpdateAsync(data));
    }

    /// <summary>
    /// 分页查询日报列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<DayWorkReportDto>>> Page([FromQuery] DayWorkReportPageReqDto request)
    {
        var pageResult = await dayWorkReportService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<DayWorkReportDto>>(pageResult));
    }

    /// <summary>
    /// 查询日报详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<DayWorkReportDto>> Detail(long id)
    {
        return Success(mapper.Map<DayWorkReportDto>(await dayWorkReportService.GetByIdAsync(id)));
    }

    /// <summary>
    /// 导出
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Export([FromBody] DayWorkReportExportReqDto request)
    {
        if (request.ExportAll)
        {
            request.PageNumber = 1;
            request.PageSize = int.MaxValue;
        }

        var list = (await dayWorkReportService.PageAsync(request)).List;
        var exportDtos = mapper.Map<List<DayWorkReportExportDto>>(list);

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

        var fileName = $"日报_{DateTime.Now:yyyyMMdd}.{fileExtension}";
        return File(fileBytes, contentType, fileName);
    }
}