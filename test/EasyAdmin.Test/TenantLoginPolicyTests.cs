using EasyAdmin.Application.Services;
using EasyAdmin.Infrastructure.Const;

namespace EasyAdmin.Test;

[TestClass]
public class TenantLoginPolicyTests
{
    [TestMethod]
    public void ResolveTenantCode_WhenMultiTenantDisabled_UsesDefaultTenant()
    {
        var result = TenantLoginPolicy.ResolveTenantCode(false, "OtherTenant");

        Assert.IsFalse(result.TenantCodeRequired);
        Assert.AreEqual(SysConst.DefaultTenantCode, result.TenantCode);
    }

    [TestMethod]
    public void ResolveTenantCode_WhenMultiTenantEnabled_TrimsAndPreservesCase()
    {
        var result = TenantLoginPolicy.ResolveTenantCode(true, "  Acme-CN  ");

        Assert.IsTrue(result.TenantCodeRequired);
        Assert.AreEqual("Acme-CN", result.TenantCode);
    }

    [TestMethod]
    public void ResolveTenantCode_WhenMultiTenantEnabledAndCodeMissing_ReturnsMissingCode()
    {
        var result = TenantLoginPolicy.ResolveTenantCode(true, "   ");

        Assert.IsTrue(result.TenantCodeRequired);
        Assert.IsNull(result.TenantCode);
    }

    [TestMethod]
    public void ResolveTenantCode_WhenCodeExceedsMaximumLength_ReturnsInvalidCode()
    {
        var result = TenantLoginPolicy.ResolveTenantCode(true, new string('a', 51));

        Assert.IsTrue(result.TenantCodeRequired);
        Assert.IsNull(result.TenantCode);
    }
}
