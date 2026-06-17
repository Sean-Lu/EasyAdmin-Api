namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 邮件发送请求
/// </summary>
public class EmailMessage
{
    /// <summary>
    /// 邮件主题
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// 邮件正文
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// 收件邮箱列表
    /// </summary>
    public List<string> ToAddresses { get; set; } = new();
}
