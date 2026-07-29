using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 文件仓储实现
/// </summary>
public class FileRepository(IConfiguration configuration, ILogger<FileRepository> logger) : BaseRepositoryExt<FileEntity>(configuration, logger), IFileRepository
{

}