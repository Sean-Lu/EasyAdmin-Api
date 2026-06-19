using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 笔记标签仓储实现
/// </summary>
public class NoteTagRepository(IConfiguration configuration, ILogger<NoteTagRepository> logger) : BaseRepositoryExt<NoteTagEntity>(configuration, logger), INoteTagRepository
{
}
