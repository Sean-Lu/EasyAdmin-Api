using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 笔记保护密码仓储实现
/// </summary>
public class UserNotePasswordRepository(IConfiguration configuration, ILogger<UserNotePasswordRepository> logger) : BaseRepositoryExt<UserNotePasswordEntity>(configuration, logger), IUserNotePasswordRepository
{
}
