using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Services;

public static class NotificationRecipientResolver
{
    public static List<long> Resolve(
        NotificationDto dto,
        IEnumerable<UserEntity> users,
        IEnumerable<UserRoleEntity> userRoles,
        IEnumerable<DepartmentEntity> departments,
        long tenantId,
        long senderUserId = 0)
    {
        var enabledUsers = users
            .Where(user => user.TenantId == tenantId && !user.IsDelete && user.State == CommonState.Enable)
            .ToList();
        var enabledUserIds = enabledUsers.Select(user => user.Id).ToHashSet();
        var recipients = new HashSet<long>();

        if (dto.SendToAll)
        {
            var allUserIds = enabledUserIds.AsEnumerable();
            if (senderUserId > 0)
            {
                allUserIds = allUserIds.Where(userId => userId != senderUserId && userId != SysConst.SuperAdminUserId);
            }
            recipients.UnionWith(allUserIds);
        }

        if (dto.UserIds?.Any() == true)
        {
            recipients.UnionWith(dto.UserIds.Where(enabledUserIds.Contains));
        }

        if (dto.RoleIds?.Any() == true)
        {
            var roleIds = dto.RoleIds.ToHashSet();
            var roleUserIds = userRoles
                .Where(userRole => userRole.TenantId == tenantId && !userRole.IsDelete && roleIds.Contains(userRole.RoleId))
                .Select(userRole => userRole.UserId)
                .Where(enabledUserIds.Contains);
            recipients.UnionWith(roleUserIds);
        }

        if (dto.DepartmentIds?.Any() == true)
        {
            var departmentIds = GetDepartmentAndChildIds(dto.DepartmentIds, departments, tenantId);
            var departmentUserIds = enabledUsers
                .Where(user => user.DepartmentId.HasValue && departmentIds.Contains(user.DepartmentId.Value))
                .Select(user => user.Id);
            recipients.UnionWith(departmentUserIds);
        }

        return recipients.OrderBy(id => id).ToList();
    }

    private static HashSet<long> GetDepartmentAndChildIds(IEnumerable<long> selectedIds, IEnumerable<DepartmentEntity> departments, long tenantId)
    {
        var tenantDepartments = departments
            .Where(department => department.TenantId == tenantId && !department.IsDelete)
            .ToList();
        var childrenMap = tenantDepartments
            .GroupBy(department => department.PId)
            .ToDictionary(group => group.Key, group => group.Select(department => department.Id).ToList());

        var result = new HashSet<long>();
        var stack = new Stack<long>(selectedIds);

        while (stack.Count > 0)
        {
            var id = stack.Pop();
            if (!result.Add(id))
            {
                continue;
            }

            if (!childrenMap.TryGetValue(id, out var childIds))
            {
                continue;
            }

            foreach (var childId in childIds)
            {
                stack.Push(childId);
            }
        }

        return result;
    }
}
