using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class CodeGenTemplateRepository(IConfiguration configuration, ILogger<CodeGenTemplateRepository> logger) : BaseRepositoryExt<CodeGenTemplateEntity>(configuration, logger), ICodeGenTemplateRepository
{
}
