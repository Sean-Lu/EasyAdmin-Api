using EasyAdmin.Domain.Entities;
using EasyAdmin.Domain.SeedData;
using EasyAdmin.Infrastructure.Const;
using TestSeedData = EasyAdmin.Domain.SeedData.Test;

namespace EasyAdmin.Test;

[TestClass]
public class SeedDataTests
{
    [TestMethod]
    public void UserSeedData_ContainsOnlyBuiltInUsers()
    {
        var users = new UserSeedData().SeedData().ToList();

        CollectionAssert.AreEquivalent(
            new[] { "superAdmin", "admin" },
            users.Select(user => user.UserName).ToArray());
    }

    [TestMethod]
    public void TestSeedData_UserSeedDataContainsAllNonBuiltInUsers()
    {
        var users = new TestSeedData.UserSeedData().SeedData().ToList();

        CollectionAssert.AreEquivalent(
            new[] { "Sean", "user01", "user02" },
            users.Select(user => user.UserName).ToArray());
        Assert.IsInstanceOfType(new TestSeedData.UserSeedData(), typeof(EasyAdmin.Domain.Contracts.ITestSeedData));
    }

    [TestMethod]
    public void UserRoleSeedData_ContainsOnlyBuiltInUserRoles()
    {
        var roles = new UserRoleSeedData().SeedData().ToList();

        CollectionAssert.AreEquivalent(
            new[] { SysConst.SuperAdminUserId, SysConst.DefaultTenantAdminUserId },
            roles.Select(role => role.UserId).ToArray());
    }

    [TestMethod]
    public void TestSeedData_UserRoleSeedDataContainsNonBuiltInUserRoles()
    {
        var roles = new TestSeedData.UserRoleSeedData().SeedData().ToList();

        CollectionAssert.AreEquivalent(new long[] { 3, 4, 10 }, roles.Select(role => role.UserId).ToArray());
        Assert.IsInstanceOfType(new TestSeedData.UserRoleSeedData(), typeof(EasyAdmin.Domain.Contracts.ITestSeedData));
    }

    [TestMethod]
    public void TestSeedDataClasses_AreMarked()
    {
        Assert.IsInstanceOfType(new TestSeedData.DepartmentSeedData(), typeof(EasyAdmin.Domain.Contracts.ITestSeedData));
        Assert.IsInstanceOfType(new TestSeedData.PositionSeedData(), typeof(EasyAdmin.Domain.Contracts.ITestSeedData));
        Assert.IsInstanceOfType(new TestSeedData.RoleSeedData(), typeof(EasyAdmin.Domain.Contracts.ITestSeedData));
        Assert.IsInstanceOfType(new TestSeedData.RoleMenuSeedData(), typeof(EasyAdmin.Domain.Contracts.ITestSeedData));
        Assert.IsInstanceOfType(new TestSeedData.ScheduleJobSeedData(), typeof(EasyAdmin.Domain.Contracts.ITestSeedData));
    }

    [TestMethod]
    public void RoleSeedData_ContainsOnlyBuiltInRoles()
    {
        var roles = new RoleSeedData().SeedData().ToList();

        CollectionAssert.AreEquivalent(new long[] { SysConst.SuperAdminRoleId, 1000001 }, roles.Select(role => role.Id).ToArray());
    }

    [TestMethod]
    public void TestSeedData_RoleSeedDataContainsNonBuiltInRoles()
    {
        var roles = new TestSeedData.RoleSeedData().SeedData().ToList();

        CollectionAssert.AreEquivalent(new long[] { 1000002, 1000003, 1000004, 1000005 }, roles.Select(role => role.Id).ToArray());
    }

    [TestMethod]
    public void RoleMenuSeedData_AssignsMinimalNormalEmployeeMenus()
    {
        var roleMenus = new TestSeedData.RoleMenuSeedData().SeedData()
            .Where(roleMenu => roleMenu.RoleId == 1000004)
            .Select(roleMenu => roleMenu.MenuId)
            .ToHashSet();

        var expectedMenuIds = new[]
        {
            1000000L,
            2000000L,
            8100001L, 8100002L, 8100003L, 8100004L, 8100005L, 8100006L, 8100007L, 8100008L, 8100009L,
            10000001L, 10000002L, 10000003L,
            11000002L, 11000003L
        };

        CollectionAssert.AreEquivalent(expectedMenuIds, roleMenus.ToArray());
        Assert.IsFalse(roleMenus.Contains(8000000));
        Assert.IsFalse(roleMenus.Contains(12000000));
        Assert.IsFalse(roleMenus.Contains(13000000));
    }
}
