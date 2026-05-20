using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Domain.Contracts;

/// <summary>
/// 更新文件仓储接口：继承通用仓储基类，管理版本文件记录的增删改查
/// </summary>
public interface IUpdateFileRepository : IBaseRepositoryExt<UpdateFileEntity>
{
}