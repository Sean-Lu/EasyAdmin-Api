using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class DepartmentRepository(IConfiguration configuration, ILogger<DepartmentRepository> logger) : BaseRepositoryExt<DepartmentEntity>(configuration, logger), IDepartmentRepository
{

}
