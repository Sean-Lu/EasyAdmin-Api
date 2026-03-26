namespace EasyAdmin.Infrastructure.Helper;

public static class TreeHelper
{
    /// <summary>
    /// 添加所有父级节点
    /// </summary>
    /// <typeparam name="T">节点类型</typeparam>
    /// <typeparam name="TKey">主键类型（如 long, int, Guid）</typeparam>
    /// <param name="nodes">起始节点集合</param>
    /// <param name="getParentFunc">获取父级的方法（通常是数据库查询）</param>
    /// <param name="getIdFunc">获取节点ID的方法</param>
    /// <param name="getPIdFunc">获取父节点ID的方法</param>
    /// <param name="isRootFunc">判断是否为顶级节点的方法</param>
    public static async Task AddAllParentsAsync<T, TKey>(
        List<T> nodes,
        Func<TKey, Task<T>> getParentFunc,
        Func<T, TKey> getIdFunc,
        Func<T, TKey> getPIdFunc,
        Func<T, bool> isRootFunc = null) where TKey : struct
    {
        var parentList = await GetAllParentsAsync(nodes, getParentFunc, getIdFunc, getPIdFunc, isRootFunc);
        if (parentList.Any())
        {
            nodes.AddRange(parentList);
        }
    }

    /// <summary>
    /// 获取所有父级节点
    /// </summary>
    /// <typeparam name="T">节点类型</typeparam>
    /// <typeparam name="TKey">主键类型（如 long, int, Guid）</typeparam>
    /// <param name="nodes">起始节点集合</param>
    /// <param name="getParentFunc">获取父级的方法（通常是数据库查询）</param>
    /// <param name="getIdFunc">获取节点ID的方法</param>
    /// <param name="getPIdFunc">获取父节点ID的方法</param>
    /// <param name="isRootFunc">判断是否为顶级节点的方法</param>
    /// <returns>所有上层节点的集合（不包含起始节点）</returns>
    public static async Task<List<T>> GetAllParentsAsync<T, TKey>(
        IEnumerable<T> nodes,
        Func<TKey, Task<T>> getParentFunc,
        Func<T, TKey> getIdFunc,
        Func<T, TKey> getPIdFunc,
        Func<T, bool> isRootFunc = null) where TKey : struct
    {
        var allParents = new List<T>();

        // 用于去重：记录已经处理过的节点ID，防止重复查库或死循环
        var processedIds = new HashSet<TKey>();

        // 初始化队列，把传入的起始节点放进去
        var queue = new Queue<T>();
        foreach (var node in nodes)
        {
            if (node != null)
            {
                queue.Enqueue(node);
                processedIds.Add(getIdFunc(node));
            }
        }

        // 开始广度优先遍历（BFS）
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var pId = getPIdFunc(current);

            // 1. 判断是否为顶级节点
            // 2. 判断是否已经处理过（防止脏数据导致的死循环）
            if (isRootFunc?.Invoke(current) == true || processedIds.Contains(pId))
            {
                continue;
            }

            // 调用传入的查询方法获取父级
            var parent = await getParentFunc(pId);
            if (parent != null)
            {
                allParents.Add(parent);
                processedIds.Add(getIdFunc(parent));

                // 将找到的父级放入队列，继续向上查找
                queue.Enqueue(parent);
            }
        }

        return allParents;
    }
}
