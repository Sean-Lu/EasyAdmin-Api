using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Test;

[TestClass]
public class NotificationRecipientResolverTests
{
    private static readonly List<UserEntity> Users = new()
    {
        new UserEntity { Id = 1, TenantId = 1, UserName = "admin", NickName = "Admin", State = CommonState.Enable },
        new UserEntity { Id = 2, TenantId = 1, UserName = "u2", NickName = "User2", DepartmentId = 10, State = CommonState.Enable },
        new UserEntity { Id = 3, TenantId = 1, UserName = "u3", NickName = "User3", DepartmentId = 11, State = CommonState.Enable },
        new UserEntity { Id = 4, TenantId = 1, UserName = "u4", NickName = "User4", DepartmentId = 12, State = CommonState.Disable },
        new UserEntity { Id = 5, TenantId = 2, UserName = "other", NickName = "Other", DepartmentId = 10, State = CommonState.Enable }
    };

    private static readonly List<UserRoleEntity> UserRoles = new()
    {
        new UserRoleEntity { Id = 100, TenantId = 1, UserId = 2, RoleId = 20 },
        new UserRoleEntity { Id = 101, TenantId = 1, UserId = 3, RoleId = 21 },
        new UserRoleEntity { Id = 102, TenantId = 2, UserId = 5, RoleId = 20 }
    };

    private static readonly List<DepartmentEntity> Departments = new()
    {
        new DepartmentEntity { Id = 10, TenantId = 1, Name = "Root", PId = 0, State = CommonState.Enable },
        new DepartmentEntity { Id = 11, TenantId = 1, Name = "Child", PId = 10, State = CommonState.Enable },
        new DepartmentEntity { Id = 12, TenantId = 1, Name = "GrandChild", PId = 11, State = CommonState.Enable },
        new DepartmentEntity { Id = 13, TenantId = 2, Name = "Other", PId = 10, State = CommonState.Enable }
    };

    [TestMethod]
    public void Resolve_Returns_All_Enabled_Tenant_Users_When_SendToAll()
    {
        var dto = new NotificationDto { SendToAll = true };

        var result = NotificationRecipientResolver.Resolve(dto, Users, UserRoles, Departments, 1);

        CollectionAssert.AreEquivalent(new List<long> { 1, 2, 3 }, result);
    }

    [TestMethod]
    public void Resolve_Excludes_SuperAdmin_And_Sender_When_SendToAll()
    {
        var dto = new NotificationDto { SendToAll = true };

        var result = NotificationRecipientResolver.Resolve(dto, Users, UserRoles, Departments, 1, senderUserId: 2);

        CollectionAssert.AreEquivalent(new List<long> { 3 }, result);
    }

    [TestMethod]
    public void Resolve_Filters_Selected_Users_To_Enabled_Current_Tenant_Users()
    {
        var dto = new NotificationDto { UserIds = new List<long> { 2, 4, 5, 999 } };

        var result = NotificationRecipientResolver.Resolve(dto, Users, UserRoles, Departments, 1);

        CollectionAssert.AreEquivalent(new List<long> { 2 }, result);
    }

    [TestMethod]
    public void Resolve_Adds_Users_In_Selected_Roles()
    {
        var dto = new NotificationDto { RoleIds = new List<long> { 20 } };

        var result = NotificationRecipientResolver.Resolve(dto, Users, UserRoles, Departments, 1);

        CollectionAssert.AreEquivalent(new List<long> { 2 }, result);
    }

    [TestMethod]
    public void Resolve_Adds_Users_In_Department_And_Child_Departments()
    {
        var dto = new NotificationDto { DepartmentIds = new List<long> { 10 } };

        var result = NotificationRecipientResolver.Resolve(dto, Users, UserRoles, Departments, 1);

        CollectionAssert.AreEquivalent(new List<long> { 2, 3 }, result);
    }

    [TestMethod]
    public void Resolve_Deduplicates_When_User_Matches_Multiple_Targets()
    {
        var dto = new NotificationDto
        {
            UserIds = new List<long> { 2 },
            RoleIds = new List<long> { 20 },
            DepartmentIds = new List<long> { 10 }
        };

        var result = NotificationRecipientResolver.Resolve(dto, Users, UserRoles, Departments, 1);

        CollectionAssert.AreEquivalent(new List<long> { 2, 3 }, result);
    }
}
