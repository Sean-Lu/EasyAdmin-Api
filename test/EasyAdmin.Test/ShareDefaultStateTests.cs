using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Test;

[TestClass]
public class ShareDefaultStateTests
{
    [TestMethod]
    public void NewShareDefaultsToDisabled()
    {
        Assert.IsFalse(new ShareSaveDto().IsEnabled);
        Assert.IsFalse(new ShareEntity().IsEnabled);
    }
}