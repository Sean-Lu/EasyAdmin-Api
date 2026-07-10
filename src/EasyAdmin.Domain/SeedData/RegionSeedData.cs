using System.Reflection;
using System.Text.Json;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.SeedData;

/// <summary>
/// 行政区划种子数据
/// </summary>
public class RegionSeedData : IEntitySeedData<RegionEntity>
{
    public IEnumerable<RegionEntity> SeedData()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "EasyAdmin.Domain.SeedData.Data.region-data.json";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            return Array.Empty<RegionEntity>();
        }

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        var items = JsonSerializer.Deserialize<List<RegionSeedItem>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (items == null || items.Count == 0)
        {
            return Array.Empty<RegionEntity>();
        }

        return items.Select(x => new RegionEntity
        {
            Id = x.Id,
            PId = x.PId,
            Name = x.Name,
            Code = x.Code,
            Level = x.Level,
            Sort = x.Sort,
            State = CommonState.Enable
        });
    }

    private class RegionSeedItem
    {
        public long Id { get; set; }
        public long PId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Sort { get; set; }
    }
}
