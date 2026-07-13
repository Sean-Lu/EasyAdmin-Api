using Microsoft.Extensions.Configuration;
using Sean.Core.Redis;

namespace EasyAdmin.Test;

internal static class RedisTestSetup
{
    private static readonly object SyncRoot = new();
    private static bool initialized;

    public static void EnsureInitialized()
    {
        if (initialized) return;

        lock (SyncRoot)
        {
            if (initialized) return;

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Redis:EndPoints"] = "127.0.0.1:6379",
                    ["Redis:Password"] = string.Empty,
                    ["Redis:DefaultSerializeType"] = "0"
                })
                .Build();

            RedisManager.Initialize(configuration);
            initialized = true;
        }
    }
}
