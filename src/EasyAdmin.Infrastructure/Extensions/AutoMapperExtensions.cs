using AutoMapper;
using EasyAdmin.Infrastructure.Helper;

namespace EasyAdmin.Infrastructure.Extensions;

public static class AutoMapperExtensions
{
    /// <summary>
    /// 类型映射，默认字段名字一一对应【允许少映射多】
    /// </summary>
    /// <typeparam name="TDestination">转化之后的model，可以理解为viewmodel</typeparam>
    /// <typeparam name="TSource">要被转化的实体，Entity</typeparam>
    /// <param name="source">可以使用这个扩展方法的类型，任何引用类型</param>
    /// <param name="actionMapping">自定义映射规则</param>
    /// <returns>转化之后的实体</returns>
    public static TDestination MapTo<TDestination, TSource>(this TSource source, Action<IMappingExpression<TSource, TDestination>> actionMapping = null)
        where TDestination : class
        where TSource : class
    {
        if (source == null) return null;
        var mapper = AutoMapperHelper.CreateMapper(actionMapping);
        return mapper?.Map<TDestination>(source);
    }
    /// <summary>
    /// 集合列表类型映射，默认字段名字一一对应【允许少映射多】
    /// </summary>
    /// <typeparam name="TDestination">转化之后的model，可以理解为viewmodel</typeparam>
    /// <typeparam name="TSource">要被转化的实体，Entity</typeparam>
    /// <param name="source">可以使用这个扩展方法的类型，任何引用类型</param>
    /// <param name="actionMapping">自定义映射规则</param>
    /// <returns>转化之后的实体列表</returns>
    public static IEnumerable<TDestination> MapToList<TDestination, TSource>(this IEnumerable<TSource> source, Action<IMappingExpression<TSource, TDestination>> actionMapping = null)
        where TDestination : class
        where TSource : class
    {
        if (source == null) return null;
        var mapper = AutoMapperHelper.CreateMapper(actionMapping);
        return mapper?.Map<List<TDestination>>(source);
    }
}