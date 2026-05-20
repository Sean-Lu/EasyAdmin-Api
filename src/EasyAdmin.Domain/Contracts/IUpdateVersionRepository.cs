using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Domain.Contracts;

/// <summary>
/// 更新版本仓储接口：继承通用仓储基类，自动拥有增删改查及分页查询能力
/// </summary>
public interface IUpdateVersionRepository : IBaseRepositoryExt<UpdateVersionEntity>
{
}