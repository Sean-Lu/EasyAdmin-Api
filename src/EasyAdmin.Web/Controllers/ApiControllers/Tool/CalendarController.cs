using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using EasyAdmin.Web.Extensions;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 日历：签到
/// </summary>
public class CalendarController(
    ILogger<CalendarController> logger,
    IMapper mapper,
    ICheckInLogService checkInLogService,
    ICheckInCountService checkInCountService
    ) : BaseApiController
{
    /// <summary>
    /// 打卡签到
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult> CheckIn([FromBody] JObject? data)
    {
        if (data == null)
        {
            return Fail("参数错误");
        }

        var checkInType = (CheckInType)(data["checkintype"]?.Value<int>() ?? 0);

        if (await checkInLogService.IsCheckInTodayAsync(UserId, checkInType))
        {
            return Fail("今日已经签到");
        }

        var model = new CheckInLogDto
        {
            CheckInType = checkInType,
            CheckInTime = DateTime.Now,
            IP = HttpContext.GetClientIp()
        };

        var insertResult = await checkInLogService.AddAsync(model);
        if (!insertResult)
        {
            return Fail("添加签到记录失败");
        }

        var checkInCountEntity = await checkInCountService.GetAsync(UserId, checkInType);
        if (checkInCountEntity == null || checkInCountEntity.UserId < 1)
        {
            //没有记录：新增记录（数据初始化）
            var entity = new CheckInCountDto
            {
                CheckInType = checkInType,
                LastCheckInTime = model.CheckInTime,
                ContinuousCheckInDays = 1
            };
            insertResult = await checkInCountService.AddAsync(entity);
            if (!insertResult)
            {
                return Fail("添加签到统计信息失败");
            }
        }
        else
        {
            //有记录：更新记录
            var intervalDays = (model.CheckInTime.Date - checkInCountEntity.LastCheckInTime.Date).TotalDays;
            var incrCount = intervalDays == 1 ? 1 : 0;
            var updateResult = await checkInCountService.IncrAsync(UserId, checkInType, model.CheckInTime, incrCount, intervalDays > 1);
            if (!updateResult)
            {
                return Fail("更新签到统计信息失败");
            }
        }

        return Success();
    }

    /// <summary>
    /// 查询指定月份的签到数据
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <param name="checkInType">签到类型</param>
    /// <param name="onlyCurMonth">是否只查询当前月份的数据</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult> Search(int year, int month, CheckInType checkInType = CheckInType.Working, bool onlyCurMonth = true)
    {
        var curMonthSearchResult = await checkInLogService.SearchAsync(UserId, checkInType, year, month);
        if (curMonthSearchResult == null)
        {
            return Fail("获取签到数据失败");
        }

        var lastMonthSearchResult = new List<CheckInLogEntity>();
        var nextMonthSearchResult = new List<CheckInLogEntity>();
        if (!onlyCurMonth)
        {
            var date = new DateTime(year, month, 1);

            #region 上一个月
            var lastMonth = date.AddMonths(-1);
            lastMonthSearchResult = await checkInLogService.SearchAsync(UserId, checkInType, lastMonth.Year, lastMonth.Month) ?? new List<CheckInLogEntity>();
            #endregion

            #region 下一个月
            var nextMonth = date.AddMonths(1);
            nextMonthSearchResult = await checkInLogService.SearchAsync(UserId, checkInType, nextMonth.Year, nextMonth.Month) ?? new List<CheckInLogEntity>();
            #endregion
        }

        var checkInCountEntity = await checkInCountService.GetAsync(UserId, checkInType);
        if (checkInCountEntity != null && checkInCountEntity.UserId > 0 && checkInCountEntity.ContinuousCheckInDays > 0)
        {
            var intervalDays = (DateTime.Now.Date - checkInCountEntity.LastCheckInTime.Date).TotalDays;
            if (intervalDays > 1)
            {
                if (!await checkInCountService.InitContinuousCheckInDaysAsync(checkInCountEntity.UserId, checkInCountEntity.CheckInType))
                {
                    return Fail("初始化连续签到天数失败");
                }
                checkInCountEntity = await checkInCountService.GetAsync(UserId, checkInType);
            }
        }

        return Success(data: new
        {
            CurMonthCheckinDatas = curMonthSearchResult.Select(c => c.CheckInTime.ToString("yyyy-MM-dd")).Distinct().ToList(),// 本月的签到数据
            LastMonthCheckinDatas = lastMonthSearchResult.Select(c => c.CheckInTime.ToString("yyyy-MM-dd")).Distinct().ToList(),// 上一个月的签到数据
            NextMonthCheckinDatas = nextMonthSearchResult.Select(c => c.CheckInTime.ToString("yyyy-MM-dd")).Distinct().ToList(),// 下一个月的签到数据
            ContinuousCheckInDays = checkInCountEntity?.ContinuousCheckInDays ?? 0// 连续签到天数
        });
    }

    /// <summary>
    /// 检查今天是否已经签到
    /// </summary>
    /// <param name="checkInType"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult> IsCheckInToday(CheckInType checkInType = CheckInType.Working)
    {
        var isCheckIn = await checkInLogService.IsCheckInTodayAsync(UserId, checkInType);
        return Success(new
        {
            IsCheckIn = isCheckIn
        });
    }
}