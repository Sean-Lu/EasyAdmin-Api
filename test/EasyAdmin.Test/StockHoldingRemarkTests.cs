using System.ComponentModel.DataAnnotations;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Test;

[TestClass]
public class StockHoldingRemarkTests
{
    [TestMethod]
    public void StockHolding_Remark_IsOptionalAndLimitedTo500Characters()
    {
        AssertRemarkProperty(typeof(StockHoldingEntity));
        AssertRemarkProperty(typeof(StockHoldingDto));
        AssertRemarkProperty(typeof(StockHoldingUpdateDto));
    }

    private static void AssertRemarkProperty(Type type)
    {
        var property = type.GetProperty("Remark");

        Assert.IsNotNull(property);
        Assert.AreEqual(typeof(string), property.PropertyType);
        var maxLength = property.GetCustomAttributes(typeof(MaxLengthAttribute), true).Cast<MaxLengthAttribute>().SingleOrDefault();
        Assert.IsNotNull(maxLength);
        Assert.AreEqual(500, maxLength.Length);
    }
}
