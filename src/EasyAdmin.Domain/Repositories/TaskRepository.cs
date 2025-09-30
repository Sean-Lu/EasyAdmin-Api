using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class TaskRepository(IConfiguration configuration, ILogger<TaskRepository> logger) : BaseRepositoryExt<TaskEntity>(configuration, logger), ITaskRepository
{

}