using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using EasyAdmin.Domain.Entities;
using Sean.Core.DbRepository;

namespace EasyAdmin.Test;

[TestClass]
public class EntityStringLengthTests
{
    [TestMethod]
    public void AiEntities_HaveBoundedExternalText()
    {
        Assert.AreEqual(200, MaxLength<AiConversationEntity>(nameof(AiConversationEntity.Title)));
        Assert.AreEqual(20000, MaxLength<AiMessageEntity>(nameof(AiMessageEntity.Content)));
        Assert.AreEqual(500, MaxLength<AiSourceEntity>(nameof(AiSourceEntity.Excerpt)));
        Assert.AreEqual(20000, MaxLength<AiDraftEntity>(nameof(AiDraftEntity.ContentJson)));
        Assert.AreEqual(100, MaxLength<AiUsageEntity>(nameof(AiUsageEntity.ErrorType)));
    }

    [TestMethod]
    public void AiTenantEntities_HaveTenantAndUserOwnership()
    {
        AssertTenantUserOwnership<AiConversationEntity>();
        AssertTenantUserOwnership<AiMessageEntity>();
        AssertTenantUserOwnership<AiSourceEntity>();
        AssertTenantUserOwnership<AiDraftEntity>();
        AssertTenantUserOwnership<AiUsageEntity>();
    }

    [TestMethod]
    public void AiModelConfig_ConfigKey_IsFixedToDefault()
    {
        var config = new AiModelConfigEntity();
        var configKey = typeof(AiModelConfigEntity).GetProperty(nameof(AiModelConfigEntity.ConfigKey));

        Assert.AreEqual("default", config.ConfigKey);
        Assert.IsNotNull(configKey);
        Assert.IsFalse(configKey.SetMethod?.IsPublic ?? false);
    }

    [TestMethod]
    public void CodeFirstMappedStringProperties_HaveMaxLength()
    {
        var missingMaxLength = typeof(EntityBase).Assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.GetCustomAttribute<CodeFirstAttribute>() != null)
            .SelectMany(type => type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.PropertyType == typeof(string) &&
                                   property.GetCustomAttribute<NotMappedAttribute>() == null)
                .Select(property => new { Entity = type.Name, Property = property.Name, MaxLength = property.GetCustomAttribute<MaxLengthAttribute>() }))
            .Where(item => item.MaxLength == null)
            .Select(item => $"{item.Entity}.{item.Property}")
            .ToList();

        Assert.IsEmpty(missingMaxLength, $"以下 CodeFirst 字符串属性未设置 MaxLength: {string.Join(", ", missingMaxLength)}");
    }

    private static int? MaxLength<TEntity>(string propertyName)
    {
        return typeof(TEntity).GetProperty(propertyName)?.GetCustomAttribute<MaxLengthAttribute>()?.Length;
    }

    private static void AssertTenantUserOwnership<TEntity>() where TEntity : TenantEntityBase
    {
        Assert.AreEqual(typeof(long), typeof(TEntity).GetProperty(nameof(AiConversationEntity.UserId))?.PropertyType);
    }
}
