using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Test;

[TestClass]
public class FileManagerAccessPolicyTests
{
    [TestMethod]
    public void CurrentUserScope_RejectsFilesOwnedByOtherUsers()
    {
        var scope = FileManagerAccessPolicy.BuildCurrentUserScope(tenantId: 1, userId: 10).Compile();

        Assert.IsTrue(scope(new FileEntity { Id = 1, TenantId = 1, CreateUserId = 10 }));
        Assert.IsFalse(scope(new FileEntity { Id = 2, TenantId = 1, CreateUserId = 11 }));
        Assert.IsFalse(scope(new FileEntity { Id = 3, TenantId = 2, CreateUserId = 10 }));
        Assert.IsFalse(scope(new FileEntity { Id = 4, TenantId = 1, CreateUserId = 10, IsDelete = true }));
    }

    [TestMethod]
    public void CanAccess_RejectsFilesOwnedByOtherUsers()
    {
        var file = new FileEntity { Id = 1, TenantId = 1, CreateUserId = 11 };

        Assert.IsFalse(FileManagerAccessPolicy.CanAccess(file, tenantId: 1, userId: 10));
    }
}