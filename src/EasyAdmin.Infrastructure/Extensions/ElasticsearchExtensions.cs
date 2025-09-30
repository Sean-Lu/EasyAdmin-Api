using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using ExistsResponse = Elastic.Clients.Elasticsearch.IndexManagement.ExistsResponse;

namespace EasyAdmin.Infrastructure.Extensions;

public static class ElasticsearchExtensions
{
    /// <summary>
    /// 创建索引
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="elasticClient"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static async Task<CreateIndexResponse> CreateIndexAsync<T>(this ElasticsearchClient elasticClient, IndexName index) where T : class
    {
        return await elasticClient.Indices.CreateAsync(index);
    }
    /// <summary>
    /// 删除索引
    /// </summary>
    /// <param name="elasticClient"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static async Task<DeleteIndexResponse> DeleteIndexAsync(this ElasticsearchClient elasticClient, IndexName index)
    {
        return await elasticClient.Indices.DeleteAsync(index);
    }
    /// <summary>
    /// 索引是否存在
    /// </summary>
    /// <param name="elasticClient"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static async Task<ExistsResponse> IndexExistsAsync(this ElasticsearchClient elasticClient, IndexName index)
    {
        return await elasticClient.Indices.ExistsAsync(index);
    }
    /// <summary>
    /// 插入数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="elasticClient"></param>
    /// <param name="data"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static async Task<IndexResponse> IndexAsync<T>(this ElasticsearchClient elasticClient, T data, IndexName index = null) where T : class
    {
        return await elasticClient.IndexAsync(data, c => c.Index(index));
    }

    /// <summary>
    /// 批量插入数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="elasticClient"></param>
    /// <param name="index"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public static bool BulkAll<T>(this ElasticsearchClient elasticClient, IndexName index, IEnumerable<T> list) where T : class
    {
        const int size = 1000;
        var tokenSource = new CancellationTokenSource();

        var observableBulk = elasticClient.BulkAll(list, f => f
                .MaxDegreeOfParallelism(8)
                .BackOffTime(TimeSpan.FromSeconds(10))
                .BackOffRetries(2)
                .Size(size)
                .RefreshOnCompleted()
                .Index(index)
                .BufferToBulk((r, buffer) => r.IndexMany(buffer))
            , tokenSource.Token);

        var countdownEvent = new CountdownEvent(1);

        Exception exception = null;

        var bulkAllObserver = new BulkAllObserver(
            onNext: response =>
            {
                //Console.WriteLine($"Indexed {response.Page * size} with {response.Retries} retries");
            },
            onError: ex =>
            {
                //Console.WriteLine("BulkAll Error : {0}", ex);
                exception = ex;
                countdownEvent.Signal();
            },
            onCompleted: () =>
            {
                //Console.WriteLine("BulkAll Finished");
                countdownEvent.Signal();
            });

        observableBulk.Subscribe(bulkAllObserver);

        countdownEvent.Wait(tokenSource.Token);

        return exception == null;
    }

    /// <summary>
    /// 转换为ES的字段名（首字母小写）
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToEsFieldName(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (value.Contains("."))
        {
            var list = value.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.FirstCharToLower()).ToList();
            return string.Join(".", list);
        }

        return value.FirstCharToLower();
    }

    /// <summary>
    /// 首字母小写
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string FirstCharToLower(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }
        if (value.Length == 1)
        {
            return value.ToLower();
        }

        return value.Substring(0, 1).ToLower() + value.Substring(1);
    }
}