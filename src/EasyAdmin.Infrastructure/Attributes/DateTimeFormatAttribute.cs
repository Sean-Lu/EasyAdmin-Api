namespace EasyAdmin.Infrastructure.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DateTimeFormatAttribute : Attribute
{
    public string Format { get; }

    public DateTimeFormatAttribute(string format)
    {
        Format = format;
    }
}