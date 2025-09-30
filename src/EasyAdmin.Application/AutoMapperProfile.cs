using AutoMapper;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Converter;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        #region 自定义类型转换
        //CreateMap<DateTime, long>().ConvertUsing<DateTimeToLongConverter>();
        //CreateMap<DateTime?, long>().ConvertUsing<DateTimeNullableToLongConverter>();
        //CreateMap<long, DateTime>().ConvertUsing<LongToDateTimeConverter>();
        //CreateMap<long, DateTime?>().ConvertUsing<LongToDateTimeNullableConverter>();
        //CreateMap<string, string>().ConvertUsing<StringNullToEmpty>();
        //CreateMap<int, bool>().ConvertUsing<IntToBooleanConverter>();
        //CreateMap<bool, int>().ConvertUsing<BooleanToIntConverter>();
        #endregion

        #region EntityMapper
        //CreateMap<CheckInLogEntity, CheckInLogDto>().ReverseMap();
        #endregion

        #region ModelMapper
        CreateMap(typeof(PageQueryResult<>), typeof(ApiResultPageData<>));
        #endregion
    }
}