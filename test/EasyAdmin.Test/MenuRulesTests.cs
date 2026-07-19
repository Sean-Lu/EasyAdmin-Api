using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Test;

[TestClass]
public class MenuRulesTests
{
    [TestMethod]
    public void NormalizeAndValidate_DirectoryClearsNavigationFields()
    {
        var menu = new MenuEntity
        {
            Type = MenuType.Directory,
            Path = "/fake",
            OutLink = "https://example.com",
            OutLinkOpenType = OutLinkOpenType.Blank
        };

        MenuRules.NormalizeAndValidate(menu);

        Assert.IsNull(menu.Path);
        Assert.IsNull(menu.OutLink);
        Assert.IsNull(menu.OutLinkOpenType);
    }

    [TestMethod]
    public void NormalizeAndValidate_InternalRequiresAbsoluteApplicationPath()
    {
        Assert.Throws<ExplicitException>(() => MenuRules.NormalizeAndValidate(new MenuEntity
        {
            Type = MenuType.Internal
        }));
        Assert.Throws<ExplicitException>(() => MenuRules.NormalizeAndValidate(new MenuEntity
        {
            Type = MenuType.Internal,
            Path = "system/user"
        }));
        Assert.Throws<ExplicitException>(() => MenuRules.NormalizeAndValidate(new MenuEntity
        {
            Type = MenuType.Internal,
            Path = "/link/custom"
        }));
    }

    [TestMethod]
    public void NormalizeAndValidate_ExternalRequiresSemanticLinkPath()
    {
        Assert.Throws<ExplicitException>(() => MenuRules.NormalizeAndValidate(new MenuEntity
        {
            Type = MenuType.External,
            Path = "/link/a/b",
            OutLink = "https://example.com",
            OutLinkOpenType = OutLinkOpenType.Blank
        }));
    }

    [TestMethod]
    public void NormalizeAndValidate_ExternalAllowsUppercaseAndUnderscore()
    {
        var menu = new MenuEntity
        {
            Type = MenuType.External,
            Path = " /link/GitHub_2 ",
            OutLink = " https://github.com/Sean-Lu ",
            OutLinkOpenType = OutLinkOpenType.Blank
        };

        MenuRules.NormalizeAndValidate(menu);

        Assert.AreEqual("/link/GitHub_2", menu.Path);
        Assert.AreEqual("https://github.com/Sean-Lu", menu.OutLink);
    }

    [TestMethod]
    public void NormalizeAndValidate_ExternalRequiresOpenType()
    {
        Assert.Throws<ExplicitException>(() => MenuRules.NormalizeAndValidate(new MenuEntity
        {
            Type = MenuType.External,
            Path = "/link/example",
            OutLink = "https://example.com"
        }));
    }

    [TestMethod]
    public void NormalizeAndValidate_ExternalRejectsUnknownOpenType()
    {
        Assert.Throws<ExplicitException>(() => MenuRules.NormalizeAndValidate(new MenuEntity
        {
            Type = MenuType.External,
            Path = "/link/example",
            OutLink = "https://example.com",
            OutLinkOpenType = (OutLinkOpenType)99
        }));
    }
}