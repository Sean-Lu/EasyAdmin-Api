using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 通知通道分发请求
/// </summary>
public class NotificationChannelDispatchRequest
{
    /// <summary>
    /// 通知标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 通知内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 是否发送邮件
    /// </summary>
    public bool SendEmail { get; set; }

    /// <summary>
    /// 是否发送短信
    /// </summary>
    public bool SendSms { get; set; }

    /// <summary>
    /// 接收用户列表
    /// </summary>
    public List<UserEntity> Recipients { get; set; } = new();
}
