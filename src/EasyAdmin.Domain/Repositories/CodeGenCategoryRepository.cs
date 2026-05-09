using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class CodeGenCategoryRepository(IConfiguration configuration, ILogger<CodeGenCategoryRepository> logger) 
    : BaseRepositoryExt<CodeGenCategoryEntity>(configuration, logger), ICodeGenCategoryRepository
{
}