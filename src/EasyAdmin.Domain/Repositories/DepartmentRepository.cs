using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 部门仓储实现
/// </summary>
public class DepartmentRepository(IConfiguration configuration, ILogger<DepartmentRepository> logger) : BaseRepositoryExt<DepartmentEntity>(configuration, logger), IDepartmentRepository
{

}
