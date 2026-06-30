using System.Linq.Expressions;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Infrastructure.Tenant;

namespace EasyAdmin.Domain.Repositories;

public static class TenantScope
{
    public static Expression<Func<TEntity, bool>> Apply<TEntity>(Expression<Func<TEntity, bool>> predicate)
    {
        var tenantId = TenantContextHolder.TenantId;
        if (tenantId < 1 || !typeof(ITenantEntityBase).IsAssignableFrom(typeof(TEntity)))
        {
            return predicate;
        }

        var tenantProperty = Expression.Property(predicate.Parameters[0], nameof(ITenantEntityBase.TenantId));
        var tenantCondition = Expression.Equal(tenantProperty, Expression.Constant(tenantId));
        return Expression.Lambda<Func<TEntity, bool>>(
            Expression.AndAlso(predicate.Body, tenantCondition),
            predicate.Parameters);
    }
}
