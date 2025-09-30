namespace EasyAdmin.Test.Models;

public class ElasticsearchTestModel
{
    public long Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string Address { get; set; }
    public DateTime CreateTime { get; set; }

    public ElasticsearchTestDetailModel CusConfig { get; set; }
}

public class ElasticsearchTestDetailModel
{
    public string Key { get; set; }
    public string Value { get; set; }
}