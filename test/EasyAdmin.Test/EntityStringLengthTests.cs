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
}
