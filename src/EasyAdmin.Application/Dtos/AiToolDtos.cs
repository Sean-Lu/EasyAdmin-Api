using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// AI工具执行结果
/// </summary>
public sealed class AiToolExecutionResult
{
    /// <summary>
    /// 工具结果
    /// </summary>
    public string Json { get; set; } = "[]";

    /// <summary>
    /// 引用来源
    /// </summary>
    public IReadOnlyList<AiSourceCandidate> Sources { get; set; } = [];
}

/// <summary>
/// AI引用候选
/// </summary>
public sealed class AiSourceCandidate
{
    /// <summary>
    /// 来源类型
    /// </summary>
    public AiSourceType SourceType { get; set; }

    /// <summary>
    /// 业务记录ID
    /// </summary>
    public long RecordId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 来源日期
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// 内容摘录
    /// </summary>
    public string Excerpt { get; set; } = string.Empty;

    /// <summary>
    /// 打开路径
    /// </summary>
    public string Route { get; set; } = string.Empty;
}