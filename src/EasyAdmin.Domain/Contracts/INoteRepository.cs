using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Domain.Contracts;

/// <summary>
/// 笔记仓储接口
/// </summary>
public interface INoteRepository : IBaseRepositoryExt<NoteEntity>
{
}
