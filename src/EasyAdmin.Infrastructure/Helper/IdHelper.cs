using Sean.Utility.SnowFlake;

namespace EasyAdmin.Infrastructure.Helper;

public static class IdHelper
{
    private static readonly IdManager _idManager = new();

    /// <summary>
    /// 基于雪花算法生成分布式全局唯一ID
    /// </summary>
    /// <returns></returns>
    public static long NextId()
    {
        return _idManager.NextId();
    }
}