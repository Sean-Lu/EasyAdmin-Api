using System.Data;
using System.Linq.Expressions;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Helper;
using EasyAdmin.Infrastructure.Tenant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Dapper;
using Sean.Core.DbRepository.Extensions;

namespace EasyAdmin.Domain.Repositories;

public abstract class BaseRepositoryExt(IConfiguration configuration, ILogger logger) : DapperBaseRepository(configuration)
{
    protected override void OnSqlExecuting(SqlExecutingContext context)
    {
        base.OnSqlExecuting(context);

        //logger.LogDebug($"SQL准备执行：{context.Sql}{Environment.NewLine}参数：{JsonConvert.SerializeObject(context.SqlParameter)}");
        //context.Handled = true;
    }

    protected override void OnSqlExecuted(SqlExecutedContext context)
    {
        base.OnSqlExecuted(context);

        //logger.LogDebug($"SQL已经执行({context.ExecutionElapsed}ms)：{context.Sql}{Environment.NewLine}参数：{JsonConvert.SerializeObject(context.SqlParameter)}");
        //context.Handled = true;
    }
}

public abstract class BaseRepositoryExt<TEntity>(IConfiguration configuration, ILogger logger) : DapperBaseRepository<TEntity>(configuration), IBaseRepositoryExt<TEntity> where TEntity : EntityBase, new()
{
    /// <summary>
    /// 是否逻辑删除
    /// </summary>
    protected virtual bool IsLogicallyDelete => true;

    protected override void OnSqlExecuting(SqlExecutingContext context)
    {
        base.OnSqlExecuting(context);

        //logger.LogDebug($"SQL准备执行：{context.Sql}{Environment.NewLine}参数：{JsonConvert.SerializeObject(context.SqlParameter)}");
        //context.Handled = true;
    }

    protected override void OnSqlExecuted(SqlExecutedContext context)
    {
        base.OnSqlExecuted(context);

        //logger.LogDebug($"SQL已经执行({context.ExecutionElapsed}ms)：{context.Sql}{Environment.NewLine}参数：{JsonConvert.SerializeObject(context.SqlParameter)}");
        //context.Handled = true;
    }

    protected override void BeforeEntityAdded(TEntity? entity)
    {
        if (entity == null)
        {
            return;
        }

        if (entity is EntityBase entityBase)
        {
            if (entityBase.Id < 1 && !typeof(TEntity).GetEntityInfo().FieldInfos.Find(c => c.PropertyName == nameof(EntityBase.Id))!.IsIdentityField)
            {
                entityBase.Id = IdHelper.NextId();
            }
            if (entityBase.CreateUserId < 1 && TenantContextHolder.UserId > 0)
            {
                entityBase.CreateUserId = TenantContextHolder.UserId;
            }
            entityBase.CreateTime = DateTime.Now;
        }

        if (entity is TenantEntityBase { TenantId: < 1 } tenantEntityBase && TenantContextHolder.TenantId > 0)
        {
            tenantEntityBase.TenantId = TenantContextHolder.TenantId;
        }
    }
    protected override void BeforeEntitiesAdded(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            BeforeEntityAdded(entity);
        }
    }

    protected override void BeforeEntityUpdated(TEntity? entity, ref Expression<Func<TEntity, object>>? fieldExpression)
    {
        if (entity == null)
        {
            return;
        }

        if (entity is EntityBase entityBase)
        {
            if (entityBase.UpdateUserId < 1 && TenantContextHolder.UserId > 0)
            {
                entityBase.UpdateUserId = TenantContextHolder.UserId;
            }
            entityBase.UpdateTime = DateTime.Now;

            if (fieldExpression != null)
            {
                if (!fieldExpression.IsFieldExists(Table<EntityBase>.Field(c => c.UpdateUserId)))
                {
                    fieldExpression = fieldExpression.AddFields(Table<EntityBase>.Field(c => c.UpdateUserId));
                }
                if (!fieldExpression.IsFieldExists(Table<EntityBase>.Field(c => c.UpdateTime)))
                {
                    fieldExpression = fieldExpression.AddFields(Table<EntityBase>.Field(c => c.UpdateTime));
                }
            }
        }
    }
    protected override void BeforeEntitiesUpdated(IEnumerable<TEntity> entities, ref Expression<Func<TEntity, object>>? fieldExpression)
    {
        foreach (var entity in entities)
        {
            BeforeEntityUpdated(entity, ref fieldExpression);
        }
    }

    #region 扩展方法
    public virtual bool DeleteById(long id, IDbTransaction? transaction = null)
    {
        if (IsLogicallyDelete)
        {
            return Update(new TEntity
            {
                Id = id,
                IsDelete = true
            }, entity => entity.IsDelete, transaction: transaction) > 0;
        }
        return Delete(entity => entity.Id == id, transaction) > 0;
    }
    public virtual bool DeleteByIds(IEnumerable<long> ids, IDbTransaction? transaction = null)
    {
        if (IsLogicallyDelete)
        {
            return Update(new TEntity
            {
                IsDelete = true
            }, entity => entity.IsDelete, entity => ids.Contains(entity.Id), transaction) > 0;
        }
        return Delete(entity => ids.Contains(entity.Id), transaction) > 0;
    }

    public virtual TEntity GetById(long id)
    {
        return Get(entity => entity.Id == id);
    }
    public virtual List<TEntity>? GetByIds(IEnumerable<long> ids)
    {
        return Query(entity => ids.Contains(entity.Id))?.ToList();
    }

    public virtual DateTime? GetLastUpdateTime(Expression<Func<TEntity, bool>>? whereExpression = null)
    {
        var sql = this.CreateQueryableBuilder()
            .MaxField(entity => entity.UpdateTime)
            .Where(whereExpression)
            .Build();
        return Get<DateTime?>(sql);
    }

    public virtual async Task<bool> DeleteByIdAsync(long id, IDbTransaction? transaction = null)
    {
        if (IsLogicallyDelete)
        {
            return await UpdateAsync(new TEntity
            {
                Id = id,
                IsDelete = true
            }, entity => entity.IsDelete, transaction: transaction) > 0;
        }
        return await DeleteAsync(entity => entity.Id == id, transaction) > 0;
    }
    public virtual async Task<bool> DeleteByIdsAsync(IEnumerable<long> ids, IDbTransaction? transaction = null)
    {
        if (IsLogicallyDelete)
        {
            return await UpdateAsync(new TEntity
            {
                IsDelete = true
            }, entity => entity.IsDelete, entity => ids.Contains(entity.Id), transaction) > 0;
        }
        return await DeleteAsync(entity => ids.Contains(entity.Id), transaction) > 0;
    }

    public virtual async Task<TEntity> GetByIdAsync(long id)
    {
        return await GetAsync(entity => entity.Id == id);
    }
    public virtual async Task<List<TEntity>?> GetByIdsAsync(IEnumerable<long> ids)
    {
        return (await QueryAsync(entity => ids.Contains(entity.Id)))?.ToList();
    }

    public virtual async Task<DateTime?> GetLastUpdateTimeAsync(Expression<Func<TEntity, bool>>? whereExpression = null)
    {
        var sql = this.CreateQueryableBuilder()
            .MaxField(entity => entity.UpdateTime)
            .Where(whereExpression)
            .Build();
        return await GetAsync<DateTime?>(sql);
    }
    #endregion
}