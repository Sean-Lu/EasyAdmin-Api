using EasyAdmin.Infrastructure.Extensions;
using EasyAdmin.Infrastructure.Helper;
using EasyAdmin.Test.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Configuration;
using Sean.Utility;
using Sean.Utility.Contracts;

namespace EasyAdmin.Test;

/// <summary>
/// ES
/// </summary>
public class ElasticsearchTest
{
    private readonly IConfiguration _configuration;
    private readonly ISimpleLogger<ElasticsearchTest> _logger;
    private readonly ElasticsearchClient _elasticClient;

    public ElasticsearchTest(IConfiguration configuration, ISimpleLogger<ElasticsearchTest> logger)
    {
        _configuration = configuration;
        _logger = logger;

        //var settings = new ConnectionSettings(new Uri(EsUri));
        //settings.DefaultIndex("test-*");
        //settings.DisableDirectStreaming();// 方便查看调试信息

        var esUri = configuration.GetValue<string>("Elasticsearch:Uri");
        _elasticClient = ElasticsearchHelper.CreateElasticClientSingle(new Uri(esUri), settings =>
        {
            settings.DefaultIndex("test-*");
        });
    }

    public void Execute()
    {
        //_elasticClient.DeleteIndex("test*");
        var list = new List<ElasticsearchTestModel>();
        for (int i = 0; i < 10; i++)
        {
            list.Add(new ElasticsearchTestModel
            {
                Id = i + 1,
                Age = i + 20,
                Name = $"asd{i + 20}",
                CreateTime = DateTime.Now
            });
        }

        var bulkAllResult = _elasticClient.BulkAll("test-202109", list);
        return;

        //DeleteTestData();
        //InitTestData("test-202007");
        //InitTestData("test-202008");
        //return;

        var idField = nameof(ElasticsearchTestModel.Id).ToEsFieldName();
        var timeField = nameof(ElasticsearchTestModel.CreateTime).ToEsFieldName();
        var now = DateTime.Now;
        var startTime = new DateMathExpression(DateTime.Parse("2020-09-01T00:00:00+08:00"));
        var endTime = new DateMathExpression(DateTime.Parse("2020-10-01T00:00:00+08:00"));

        //var responseHealth = _elasticClient.Cluster.Health();//查看集群健康状态
        //Console.WriteLine(responseHealth.Status);

        #region 查询示例
        //var result = PageSearch(1, 10);

        //var result = _elasticClient.Search<TestModel>(c => c.Query(q => q.Match(o => o.Field(f => f.Id).Query("101014"))));

        // Query in the Lucene query string syntax
        // 注意关键字要大写：AND\OR\...
        //var result = _elasticClient.Search<TestModel>(q => q.QueryOnQueryString("name : Sean AND (id : 101013 OR id : 101016)"));

        //var result = _elasticClient.Search<TestModel>(q => q.From(0).Size(1000).Query(p => p.DateRange(v => v.Field(timeField).GreaterThanOrEquals(startTime)) && p.DateRange(v => v.Field(timeField).LessThan(endTime))));
        //if (result.ApiCall?.RequestBodyInBytes != null)
        //{
        //    Console.WriteLine(ConvertHelper.ToString(result.ApiCall.RequestBodyInBytes, Encoding.UTF8));
        //}
        //Console.WriteLine(result.Count);

        //var result = _elasticClient.Search<TestModel>(q => q.From(0).Size(1000).Query(p => p.LongRange(v => v.Field(idField).GreaterThanOrEquals(101010)) && p.LongRange(v => v.Field(idField).LessThan(101030))));
        //Console.WriteLine(result.Count);

        //var result = _elasticClient.Search<TestModel>(c => c.From(0).Size(1000).Query(q => q.Term(t => t.Id, 101013) || q.Match(m => m.Field(f => f.Id).Query("101015"))));
        #endregion

        #region 删除示例
        //var response = _elasticClient.DeleteByQuery<TestModel>(s => s.Query(q => q.Match(m => m.Field(f => f.Id).Query("101011"))));

        //var response = _elasticClient.DeleteByQuery<TestModel>(s => s.Query(q => q.DateRange(c => c.Field(timeField).LessThan(endTime))
        //                                                                                      && (q.Match(m => m.Field(f => f.Age).Query("30"))
        //                                                                                          || q.Match(m => m.Field(f => f.Age).Query("32")))));

        //var delResponse = _elasticClient.DeleteByQuery<TestModel>(q => q.Query(p => p.DateRange(v => v.Field(timeField).LessThan(endTime))));
        //var delResponse = _elasticClient.DeleteByQuery<TestModel>(q => q.Query(p => p.LongRange(v => v.Field(idField).LessThanOrEquals(101017))));
        //Console.WriteLine($"是否成功：{delResponse.IsValid}，删除数量：{delResponse.Deleted}");
        #endregion
    }

    /// <summary>
    /// 初始化测试数据
    /// </summary>
    private async Task InitTestData(string indexName)
    {
        var model = new ElasticsearchTestModel
        {
            Id = 101010,
            Name = "Sean",
            Age = 20,
            Address = "浙江省杭州市西湖区",
            CreateTime = DateTime.Now,
            CusConfig = new ElasticsearchTestDetailModel
            {
                Key = "配置key",
                Value = "配置Value",
            }
        };

        if (!(await _elasticClient.IndexExistsAsync(indexName)).Exists)
        {
            // 创建索引：
            // number_of_shards：分片数，默认为1
            // number_of_replicas：副本数，默认为1。用于故障转移，单节点的设置为0即可，否则集群健康值会变为yellow。

            // 如果是集群部署，可跳过创建索引的步骤，因为在Index(model, c => c.Index(indexName)的时候，如果索引不存在，ES会自动创建索引
            // 这里提前先创建索引的目的是为了解决：单节点Elasticsearch出现unassigned_shards导致集群健康值变为yellow
            var createIndexRequest = new CreateIndexRequest(indexName)
            {
                Mappings = new TypeMapping
                {
                    Properties = new Properties
                    {
                        { nameof(ElasticsearchTestModel.Id).ToEsFieldName(), new KeywordProperty() },
                        { nameof(ElasticsearchTestModel.Name).ToEsFieldName(), new TextProperty() },
                        { nameof(ElasticsearchTestModel.Age).ToEsFieldName(), new IntegerNumberProperty() },
                        { nameof(ElasticsearchTestModel.Address).ToEsFieldName(), new TextProperty() },
                        { nameof(ElasticsearchTestModel.CreateTime).ToEsFieldName(), new DateProperty() },
                        { nameof(ElasticsearchTestModel.CusConfig).ToEsFieldName(), new ObjectProperty() }
                    }
                },
                Settings = new IndexSettings
                {
                    NumberOfReplicas = 0
                }
            };
            await _elasticClient.Indices.CreateAsync(createIndexRequest);
        }
        await DelegateHelper.RepeatAsync(async () =>
        {
            var response = await _elasticClient.IndexAsync(model, c => c.Index(indexName));
            model.Id++;
            model.Age++;
            model.CreateTime = model.CreateTime.AddMinutes(1);
            return false;
        }, 10);
    }

    /// <summary>
    /// 删除测试数据
    /// </summary>
    private async Task DeleteTestData()
    {
        await _elasticClient.DeleteIndexAsync("test-*");
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public async Task<SearchResponse<ElasticsearchTestModel>> PageSearchAsync(IndexName index, int pageNumber = 1, int pageSize = 10)
    {
        return await _elasticClient.SearchAsync<ElasticsearchTestModel>(c => c
            .Indices(index)
            .From((pageNumber - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q.MatchAll(new MatchAllQuery()))
            .Sort(descriptor => descriptor.Field(model => model.CreateTime, SortOrder.Desc)));
    }
}