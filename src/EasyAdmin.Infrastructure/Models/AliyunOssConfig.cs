namespace EasyAdmin.Infrastructure.Models;

/// <summary>
/// 阿里云OSS配置
/// </summary>
public class AliyunOssConfig
{
    /// <summary>
    /// 访问密钥ID
    /// </summary>
    public string AccessKeyId { get; set; }
    /// <summary>
    /// 访问密钥密钥
    /// </summary>
    public string AccessKeySecret { get; set; }
    /// <summary>
    /// 端点
    /// </summary>
    public string Endpoint { get; set; }
    /// <summary>
    /// 桶名称
    /// </summary>
    public string BucketName { get; set; }
    /// <summary>
    /// 公共域名
    /// </summary>
    public string PublicDomain { get; set; }
}