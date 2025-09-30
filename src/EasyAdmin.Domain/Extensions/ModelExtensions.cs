using EasyAdmin.Domain.Contracts;

namespace EasyAdmin.Domain.Extensions;

public static class ModelExtensions
{
    public static List<T>? ToTreeList<T>(this List<T>? list, long rootPId = 0) where T : ITreeEntityBase<T>
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