namespace EasyAdmin.Application.Services;

/// <summary>
/// 百宝箱工具定义
/// </summary>
public sealed record ToolboxToolDefinition(long Id, string Key, string Title, string Icon)
{
    /// <summary>
    /// 打开路径
    /// </summary>
    public string Path => $"{ToolboxToolCatalog.ToolboxPath}?tool={Uri.EscapeDataString(Key)}";
}

/// <summary>
/// 百宝箱工具目录
/// </summary>
public static class ToolboxToolCatalog
{
    /// <summary>
    /// 百宝箱菜单路径
    /// </summary>
    public const string ToolboxPath = "/tool/commonTools";

    /// <summary>
    /// 工具列表
    /// </summary>
    public static IReadOnlyList<ToolboxToolDefinition> All { get; } = new List<ToolboxToolDefinition>
    {
        new(1, "sqlToTable", "SQL 转表格", "BarChartOutlined"),
        new(2, "jsonToTable", "JSON 转表格", "CodeOutlined"),
        new(3, "jsonParser", "JSON 解析工具", "FileTextOutlined"),
        new(4, "urlCodec", "URL 编码/解码", "LinkOutlined"),
        new(5, "qrCode", "二维码工具", "QrcodeOutlined"),
        new(6, "randomDecision", "随机决策器", "CompassOutlined"),
        new(7, "randomPassword", "随机密码", "LockOutlined"),
        new(8, "jwtParser", "JWT 解析", "SafetyCertificateOutlined"),
        new(9, "webSocketTester", "WebSocket 测试", "ApiOutlined"),
        new(10, "lottery", "抽奖工具", "GiftOutlined"),
        new(11, "stockPortfolio", "股票持仓管理", "FundProjectionScreenOutlined"),
        new(12, "timestamp", "时间戳转换", "ClockCircleOutlined"),
        new(13, "crypto", "加解密工具", "LockOutlined"),
        new(14, "regexTester", "正则表达式测试", "CodeOutlined"),
        new(15, "cronTester", "Cron 表达式测试", "ClockCircleOutlined"),
        new(16, "timer", "计时器", "ClockCircleOutlined"),
        new(17, "countdown", "倒计时", "HourglassOutlined"),
        new(18, "flipClock", "翻页时钟", "FieldTimeOutlined")
    };

    /// <summary>
    /// 查找工具
    /// </summary>
    public static ToolboxToolDefinition? Find(long id)
    {
        return All.FirstOrDefault(item => item.Id == id);
    }
}
