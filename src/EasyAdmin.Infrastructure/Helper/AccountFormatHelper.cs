using System.Text.RegularExpressions;

namespace EasyAdmin.Infrastructure.Helper;

/// <summary>
/// 账号格式识别帮助类
/// </summary>
public static class AccountFormatHelper
{
    // 简化版手机号正则：以 1 开头的 11 位数字
    private static readonly Regex PhoneRegex = new(@"^1\d{10}$", RegexOptions.Compiled);
    // 简化版邮箱正则：非空 + 含 @ + @ 后含 .
    private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

    /// <summary>
    /// 是否为手机号格式
    /// </summary>
    public static bool IsPhoneNumber(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && PhoneRegex.IsMatch(value.Trim());
    }

    /// <summary>
    /// 是否为邮箱格式
    /// </summary>
    public static bool IsEmail(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && EmailRegex.IsMatch(value.Trim());
    }
}