using EasyAdmin.Domain.Entities;
using EasyAdmin.Domain.Repositories;
using EasyAdmin.Infrastructure.Tenant;

namespace EasyAdmin.Test;

[TestClass]
public class TenantScopeTests
{
    [TestCleanup]
    public void Cleanup() => TenantContextHolder.Clear();

    [TestMethod]
    public void Apply_FiltersTenantEntityToCurrentTenant()
    {
        TenantContextHolder.UserInfo = new() { TenantId = 2, UserId = 10 };
        var predicate = TenantScope.Apply<UserEntity>(entity => entity.Id == 7).Compile();

        Assert.IsTrue(predicate(new UserEntity { Id = 7, TenantId = 2 }));
        Assert.IsFalse(predicate(new UserEntity { Id = 7, TenantId = 3 }));
    }

    [TestMethod]
    public void Apply_DoesNotFilterNonTenantEntity()
    {
        TenantContextHolder.UserInfo = new() { TenantId = 2, UserId = 10 };
        var predicate = TenantScope.Apply<TenantEntity>(entity => entity.Id == 7).Compile();

        Assert.IsTrue(predicate(new TenantEntity { Id = 7 }));
    }
}
