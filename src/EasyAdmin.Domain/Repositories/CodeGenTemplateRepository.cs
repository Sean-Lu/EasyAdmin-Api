using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 代码生成模板仓储实现
/// </summary>
public class CodeGenTemplateRepository(IConfiguration configuration, ILogger<CodeGenTemplateRepository> logger) : BaseRepositoryExt<CodeGenTemplateEntity>(configuration, logger), ICodeGenTemplateRepository
{
}
