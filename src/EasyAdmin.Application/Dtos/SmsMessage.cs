namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 短信发送请求
/// </summary>
public class SmsMessage
{
    /// <summary>
    /// 短信标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 短信内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 接收手机号列表
    /// </summary>
    public List<string> PhoneNumbers { get; set; } = new();
}
