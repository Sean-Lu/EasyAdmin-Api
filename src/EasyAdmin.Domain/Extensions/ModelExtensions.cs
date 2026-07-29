using EasyAdmin.Domain.Contracts;

namespace EasyAdmin.Domain.Extensions;

/// <summary>
/// 模型扩展
/// </summary>
public static class ModelExtensions
{
    /// <summary>
    /// 转换为树形列表
    /// </summary>
    public static List<T> ToTreeList<T>(this List<T> list, long rootPId = 0) where T : ITreeEntityBase<T>
    {
        if (list == null || !list.Any())
        {
            return list;
        }

        var treeList = list.Where(c => c.PId == rootPId).OrderBy(c => c.Sort).ThenBy(c => c.Id).ToList();
        foreach (var item in treeList)
        {
            if (list.Any(c => c.PId == item.Id))
            {
                item.Children = list.ToTreeList(item.Id);
            }
        }
        return treeList;
    }
}