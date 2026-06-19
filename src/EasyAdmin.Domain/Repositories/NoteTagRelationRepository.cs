using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 笔记标签关系仓储实现
/// </summary>
public class NoteTagRelationRepository(IConfiguration configuration, ILogger<NoteTagRelationRepository> logger) : BaseRepositoryExt<NoteTagRelationEntity>(configuration, logger), INoteTagRelationRepository
{
}
