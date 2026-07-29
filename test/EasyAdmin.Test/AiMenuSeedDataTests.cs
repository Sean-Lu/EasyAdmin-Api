using EasyAdmin.Domain.SeedData;
using EasyAdmin.Infrastructure.Const;

namespace EasyAdmin.Test;

[TestClass]
public class AiMenuSeedDataTests
{
    [TestMethod]
    public void SeedData_ContainsUniqueAiMenusAndNormalUserAssistantPermission()
    {
        var menus = new MenuSeedData().SeedData().ToList();

        Assert.AreEqual(menus.Count, menus.Select(item => item.Id).Distinct().Count());
        Assert.AreEqual(
            menus.Where(item => item.Path != null).Count(),
            menus.Where(item => item.Path != null).Select(item => item.Path).Distinct().Count());
        Assert.AreEqual(
            SysConst.AiAssistantMenuId,
            menus.Single(item => item.Path == "/ai/assistant").Id);
        Assert.AreEqual(
            "AI 助手",
            menus.Single(item => item.Path == "/ai/assistant").Title);
        Assert.AreEqual(
            SysConst.AiConfigMenuId,
            menus.Single(item => item.Path == "/system/ai/config").Id);
        Assert.AreEqual(
            SysConst.AiUsageMenuId,
            menus.Single(item => item.Path == "/system/ai/usage").Id);
        CollectionAssert.Contains(SysConst.NormalUserMenuIds, SysConst.AiAssistantMenuId);
        CollectionAssert.DoesNotContain(SysConst.NormalUserMenuIds, SysConst.AiConfigMenuId);
        CollectionAssert.DoesNotContain(SysConst.NormalUserMenuIds, SysConst.AiUsageMenuId);

        var roleMenus = new RoleMenuSeedData().SeedData().Select(item => item.MenuId).ToList();
        CollectionAssert.Contains(roleMenus, SysConst.AiAssistantMenuId);
    }
}