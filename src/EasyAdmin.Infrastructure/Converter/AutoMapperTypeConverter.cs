using AutoMapper;

namespace EasyAdmin.Infrastructure.Converter;

/// <summary>
/// DateTime转long
/// </summary>
public class DateTimeToLongConverter : ITypeConverter<DateTime, long>
{
    public long Convert(DateTime source, long destination, ResolutionContext context)
    {
        return source.Ticks;
    }
}
/// <summary>
/// DateTime?转long
/// </summary>
public class DateTimeNullableToLongConverter : ITypeConverter<DateTime?, long>
{
    public long Convert(DateTime? source, long destination, ResolutionContext context)
    {
        return source?.Ticks ?? 0;
    }
}
/// <summary>
/// long转DateTime
/// </summary>
public class LongToDateTimeConverter : ITypeConverter<long, DateTime>
{
    public DateTime Convert(long source, DateTime destination, ResolutionContext context)
    {
        return source <= 0 ? default : new DateTime(source);
    }
}
/// <summary>
/// long转DateTime?
/// </summary>
public class LongToDateTimeNullableConverter : ITypeConverter<long, DateTime?>
{
    public DateTime? Convert(long source, DateTime? destination, ResolutionContext context)
    {
        return source <= 0 ? (DateTime?)null : new DateTime(source);
    }
}
/// <summary>
/// 字符串：null转empty
/// </summary>
public class StringNullToEmpty : ITypeConverter<string, string>
{
    public string Convert(string source, string destination, ResolutionContext context)
    {
        return source ?? string.Empty;
    }
}
/// <summary>
/// int转bool
/// </summary>
public class IntToBooleanConverter : ITypeConverter<int, bool>
{
    public bool Convert(int source, bool destination, ResolutionContext context)
    {
        return source > 0;
    }
}
/// <summary>
/// bool转int
/// </summary>
public class BooleanToIntConverter : ITypeConverter<bool, int>
{
    public int Convert(bool source, int destination, ResolutionContext context)
    {
        return source ? 1 : 0;
    }
}