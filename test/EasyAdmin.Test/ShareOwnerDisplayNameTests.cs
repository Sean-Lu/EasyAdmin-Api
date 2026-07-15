using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Test;

[TestClass]
public class ShareOwnerDisplayNameTests
{
    [TestMethod]
    public void Resolve_PrefersNicknameThenUsernameThenGenericName()
    {
        Assert.AreEqual("分享者昵称", ShareOwnerDisplayName.Resolve(new UserEntity { NickName = "分享者昵称", UserName = "owner" }));
        Assert.AreEqual("owner", ShareOwnerDisplayName.Resolve(new UserEntity { NickName = " ", UserName = "owner" }));
        Assert.AreEqual("分享者", ShareOwnerDisplayName.Resolve(null));
    }
}