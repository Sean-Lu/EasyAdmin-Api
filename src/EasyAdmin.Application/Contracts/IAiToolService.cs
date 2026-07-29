using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Ai;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// AI业务工具服务接口
/// </summary>
public interface IAiToolService
{
    /// <summary>
    /// 获取模型工具定义
    /// </summary>
    IReadOnlyList<AiToolDefinition> GetDefinitions();

    /// <summary>
    /// 执行模型工具
    /// </summary>
    Task<AiToolExecutionResult> ExecuteAsync(string name, string argumentsJson);

    /// <summary>
    /// 解析引用来源
    /// </summary>
    Task<string> ResolveSourceAsync(AiSourceType sourceType, long sourceId);
}
