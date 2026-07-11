using EasyAdmin.Infrastructure.Converter;
using EasyAdmin.Application.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace EasyAdmin.Test;

[TestClass]
public class JsonLongAsNumberConverterTests
{
    [TestMethod]
    public void Serialize_LongAsNumber()
    {
        var json = JsonConvert.SerializeObject(new NumericModel { Value = 1234567890123L });

        Assert.AreEqual("{\"Value\":1234567890123}", json);
    }

    [TestMethod]
    public void RedisServerInfo_LongFieldsRemainNumbersWithGlobalLongConverter()
    {
        var json = JsonConvert.SerializeObject(
            new RedisServerInfoDto { KeyCount = 1234567890123L },
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = { new JsonLongToStringConverter() }
            });

        var result = JObject.Parse(json);
        Assert.AreEqual(JTokenType.Integer, result["keyCount"]?.Type);
    }

    private sealed class NumericModel
    {
        [JsonConverter(typeof(JsonLongAsNumberConverter))]
        public long Value { get; set; }
    }
}
