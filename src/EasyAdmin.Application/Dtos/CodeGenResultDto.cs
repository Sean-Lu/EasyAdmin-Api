namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 代码生成结果 DTO
/// </summary>
public class CodeGenResultDto
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public string TaskId { get; set; }
    /// <summary>
    /// 生成的文件列表
    /// </summary>
    public List<CodeGenFileDto> Files { get; set; }
}