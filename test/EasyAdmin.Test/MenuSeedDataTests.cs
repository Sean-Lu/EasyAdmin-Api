using EasyAdmin.Domain.SeedData;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Test;

[TestClass]
public class MenuSeedDataTests
{
    [TestMethod]
    public void MenuSeedData_UsesExplicitMenuTypes()
    {
        var menus = new MenuSeedData().SeedData().ToList();
        var parentIds = menus.Select(menu => menu.PId).Where(id => id > 0).ToHashSet();

        foreach (var menu in menus)
        {
            if (parentIds.Contains(menu.Id))
            {
                Assert.AreEqual(MenuType.Directory, menu.Type, menu.Title);
                Assert.IsNull(menu.Path, menu.Title);
                Assert.IsNull(menu.OutLink, menu.Title);
                Assert.IsNull(menu.OutLinkOpenType, menu.Title);
                continue;
            }

            Assert.IsTrue(menu.Type is MenuType.Internal or MenuType.External, menu.Title);
        }

        var externalPaths = menus
            .Where(menu => menu.Type == MenuType.External)
            .Select(menu => menu.Path)
            .ToArray();

        CollectionAssert.AreEquivalent(
            new[] { "/link/gitee", "/link/github", "/link/baidu" },
            externalPaths);
    }
}