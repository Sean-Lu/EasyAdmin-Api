using System.Linq.Expressions;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 文件管理访问策略
/// </summary>
public static class FileManagerAccessPolicy
{
    /// <summary>
    /// 构建当前用户文件范围
    /// </summary>
    public static Expression<Func<FileEntity, bool>> BuildCurrentUserScope(long tenantId, long userId)
    {
        return entity =>
            entity.TenantId == tenantId &&
            entity.CreateUserId == userId &&
            !entity.IsDelete;
    }

    /// <summary>
    /// 判断当前用户是否可访问文件
    /// </summary>
    public static bool CanAccess(FileEntity? file, long tenantId, long userId)
    {
        return file != null &&
               file.Id > 0 &&
               file.TenantId == tenantId &&
               file.CreateUserId == userId &&
               !file.IsDelete;
    }
}