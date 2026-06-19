using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 笔记仓储实现
/// </summary>
public class NoteRepository(IConfiguration configuration, ILogger<NoteRepository> logger) : BaseRepositoryExt<NoteEntity>(configuration, logger), INoteRepository
{
}
