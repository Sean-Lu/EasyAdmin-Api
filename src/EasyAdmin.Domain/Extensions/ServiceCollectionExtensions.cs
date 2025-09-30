using System.Data.SQLite;
using System.Reflection;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.CodeFirst;
using Sean.Core.DbRepository.Extensions;
using Sean.Utility.Extensions;

namespace EasyAdmin.Domain.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 领域层依赖注入
    /// </summary>
    /// <param name="services"></param>
    public static void AddDomainDI(this IServiceCollection services)
    {
        services.AddServiceByInterfaceSuffix(Assembly.GetExecutingAssembly(), "Repository", ServiceLifetime.Transient);

        IConfiguration configuration = services.GetConfiguration();

        #region Database configuration.

        #region 配置数据库和数据库提供者工厂之间的映射关系
        DatabaseType.MySql.SetDbProviderMap(new DbProviderMap("MySql.Data.MySqlClient", MySqlClientFactory.Instance));// MySql
        //DatabaseType.SqlServer.SetDbProviderMap(new DbProviderMap("System.Data.SqlClient", SqlClientFactory.Instance));// Microsoft SQL Server
        //DatabaseType.Oracle.SetDbProviderMap(new DbProviderMap("Oracle.ManagedDataAccess.Client", OracleClientFactory.Instance));// Oracle
        DatabaseType.SQLite.SetDbProviderMap(new DbProviderMap("System.Data.SQLite", SQLiteFactory.Instance));// SQLite
        #endregion

        DbContextConfiguration.Configure(options =>
        {
            var databaseType = configuration.GetValue("DatabaseSettings:DatabaseType", DatabaseType.Unknown);
            if (databaseType == DatabaseType.SQLite)
            {
                options.SynchronousWriteOptions.Enable = true;// 启用同步写入模式：解决多线程并发写入导致的锁库问题
                options.SynchronousWriteOptions.LockTimeout = 30000;// 同步写入锁等待超时时间（单位：毫秒），默认值：10000
                options.SynchronousWriteOptions.OnLockTakenFailed = lockTimeout =>
                {
                    Console.WriteLine($"######获取同步写入锁失败({lockTimeout}ms)");
                    return true;// 返回true：继续执行（仍然可能会发生锁库问题）。返回false：不再继续执行，直接返回默认值
                };
            }

            options.BulkEntityCount = 200;
            options.SqlExecuting += OnSqlExecuting;
            options.SqlExecuted += OnSqlExecuted;
            //options.JsonSerializer = NewJsonSerializer.Instance;
        });

        #endregion

        AutoUpgradeDatabase(configuration);
    }

    private static void OnSqlExecuting(SqlExecutingContext context)
    {

    }

    private static void OnSqlExecuted(SqlExecutedContext context)
    {

    }

    private static void AutoUpgradeDatabase(IConfiguration configuration)
    {
        var isAutoUpgradeDatabase = configuration.GetValue("DatabaseSettings:AutoUpgrade", false);
        if (isAutoUpgradeDatabase)
        {
            var db = new DbFactory(configuration);

            var assembly = Assembly.GetExecutingAssembly();
            var seedDataTypes = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntitySeedData<>))).ToList();
            var seedDataMap = new Dictionary<Type, bool>();
            if (seedDataTypes.Any())
            {
                using var connection = db.OpenNewConnection(true);
                foreach (var seedDataType in seedDataTypes)
                {
                    var interfaceType = seedDataType.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntitySeedData<>));
                    var entityType = interfaceType.GenericTypeArguments[0];
                    var tableName = entityType.GetEntityInfo().TableName;
                    var sql = db.DbType.GetSqlForTableExists(connection, tableName);
                    var isTableExists = db.ExecuteScalar<int>(connection, sql) > 0;
                    if (!isTableExists)
                    {
                        seedDataMap.Add(seedDataType, false);// 只记录表不存在的
                    }
                }
            }

            IDatabaseUpgrader dbUpgrader = new DatabaseUpgrader(db);
            dbUpgrader.Upgrade(Assembly.GetExecutingAssembly());// 自动升级数据库：自动创建表、字段

            // 如果表不存在，则在创建表后自动新增种子数据
            if (seedDataMap.Any())
            {
                foreach (var kv in seedDataMap)
                {
                    var seedDataType = kv.Key;
                    var instance = Activator.CreateInstance(seedDataType);
                    var interfaceType = seedDataType.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntitySeedData<>));
                    var entityType = interfaceType.GenericTypeArguments[0];
                    var method = typeof(IEntitySeedData<>).MakeGenericType(entityType).GetMethod(nameof(IEntitySeedData<UserEntity>.SeedData));
                    var seedData = method.Invoke(instance, null) as IEnumerable<EntityBase>;
                    foreach (var data in seedData)
                    {
                        if (data.CreateUserId < 1)
                        {
                            data.CreateUserId = SysConst.SuperAdminUserId;
                        }
                        data.CreateTime ??= DateTime.Now;
                    }

                    var sqlBuilderType = typeof(InsertableSqlBuilder<>);
                    var constructedType = sqlBuilderType.MakeGenericType(entityType);
                    var createMethod = constructedType.GetMethod(nameof(InsertableSqlBuilder<UserEntity>.Create), new[] { typeof(DatabaseType) });
                    var sqlBuilderInstance = createMethod.Invoke(null, new object[] { db.DbType });
                    var setParameterMethod = constructedType.GetMethod(nameof(InsertableSqlBuilder<UserEntity>.SetParameter));
                    var sqlBuilderWithParameter = setParameterMethod.Invoke(sqlBuilderInstance, new object[] { seedData });
                    var buildMethod = constructedType.GetMethod(nameof(InsertableSqlBuilder<UserEntity>.Build));
                    var sqlCommand = buildMethod.Invoke(sqlBuilderWithParameter, null) as ISqlCommand;

                    db.ExecuteNonQuery(sqlCommand);
                }
            }
        }
    }
}