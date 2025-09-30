using AutoMapper;

namespace EasyAdmin.Infrastructure.Helper;

public static class AutoMapperHelper
{
    /// <summary>
    /// 创建IMapper
    /// </summary>
    /// <typeparam name="TDestination"></typeparam>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="actionMapping">自定义映射规则</param>
    /// <returns></returns>
    public static IMapper CreateMapper<TDestination, TSource>(Action<IMappingExpression<TSource, TDestination>> actionMapping = null)
    {
        var config = new MapperConfiguration(cfg =>
        {
            var mappingExpression = cfg.CreateMap<TSource, TDestination>();
            actionMapping?.Invoke(mappingExpression);
        }, null);
        //var config2 = new MapperConfiguration(cfg => cfg.CreateMap<SourceUser, DestUser2>()
        //        .ForMember(d => d.DestName, opt => opt.MapFrom(s => s.Name))    //指定字段一一对应
        //        .ForMember(d => d.Birthday, opt => opt.MapFrom(src => src.Birthday.ToString("yy-MM-dd HH:mm")))//指定字段，并转化指定的格式
        //        .ForMember(d => d.Age, opt => opt.Condition(src => src.Age > 5))//条件赋值
        //        .ForMember(d => d.A1, opt => opt.Ignore())//忽略该字段，不给该字段赋值
        //        .ForMember(d => d.A1, opt => opt.NullSubstitute("Default Value"))//如果源字段值为空，则赋值为 Default Value
        //        .ForMember(d => d.A1, opt => opt.MapFrom(src => src.Name + src.Age * 3 + src.Birthday.ToString("d"))));//可以自己随意组合赋值
        var mapper = config.CreateMapper();
        return mapper;
    }
}