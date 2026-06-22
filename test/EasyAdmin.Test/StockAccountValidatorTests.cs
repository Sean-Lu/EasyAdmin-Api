using EasyAdmin.Application.Services;
using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Test;

[TestClass]
public class StockAccountValidatorTests
{
    [TestMethod]
    public void Normalize_Trims_BrokerName_And_Defaults_Null_Assets_To_Zero()
    {
        var normalized = StockAccountValidator.Normalize("  华泰证券  ", null, null);

        Assert.AreEqual("华泰证券", normalized.BrokerName);
        Assert.AreEqual(0, normalized.InitialAsset);
        Assert.AreEqual(0, normalized.CurrentAsset);
    }

    [TestMethod]
    public void Normalize_Rejects_Blank_BrokerName()
    {
        var exception = AssertThrowsExplicitException(() =>
            StockAccountValidator.Normalize("   ", 0, 0));

        Assert.AreEqual("券商名称不能为空", exception.Message);
    }

    [TestMethod]
    public void Normalize_Rejects_Negative_InitialAsset()
    {
        var exception = AssertThrowsExplicitException(() =>
            StockAccountValidator.Normalize("华泰证券", -1, 0));

        Assert.AreEqual("初始资产不能小于0", exception.Message);
    }

    [TestMethod]
    public void Normalize_Rejects_Negative_CurrentAsset()
    {
        var exception = AssertThrowsExplicitException(() =>
            StockAccountValidator.Normalize("华泰证券", 0, -1));

        Assert.AreEqual("现资产不能小于0", exception.Message);
    }

    private static ExplicitException AssertThrowsExplicitException(Action action)
    {
        try
        {
            action();
        }
        catch (ExplicitException exception)
        {
            return exception;
        }

        Assert.Fail($"Expected {nameof(ExplicitException)}.");
        throw new InvalidOperationException();
    }
}
