using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace EasyAdmin.Infrastructure.Helper;

public class ElasticsearchHelper
{
    /// <summary>
    /// 单节点
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static ElasticsearchClient CreateElasticClientSingle(Uri uri, Action<ElasticsearchClientSettings> action = null)
    {
        var settings = new ElasticsearchClientSettings(uri);
        //if (!string.IsNullOrWhiteSpace(defaultIndex))
        //{
        //    settings.DefaultIndex(defaultIndex);// 注：索引名称必须小写
        //}
        //if (!string.IsNullOrWhiteSpace(username))
        //{
        //    settings.Authentication(new BasicAuthentication(username, password));
        //}
        action?.Invoke(settings);
        var client = new ElasticsearchClient(settings);
        return client;
    }

    /// <summary>
    /// 集群
    /// </summary>
    /// <param name="uriList"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static ElasticsearchClient CreateElasticClientMultiple(List<Uri> uriList, Action<ElasticsearchClientSettings> action = null)
    {
        if (uriList == null || !uriList.Any())
        {
            return CreateElasticClientSingle(null, action);
        }

        var pool = new StaticNodePool(uriList);
        var settings = new ElasticsearchClientSettings(pool);
        //if (!string.IsNullOrWhiteSpace(defaultIndex))
        //{
        //    settings.DefaultIndex(defaultIndex);
        //}
        //if (!string.IsNullOrWhiteSpace(username))
        //{
        //    settings.Authentication(new BasicAuthentication(username, password));
        //}
        action?.Invoke(settings);
        var client = new ElasticsearchClient(settings);
        return client;
    }
}