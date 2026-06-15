using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 随机决策候选项服务接口
/// </summary>
public interface IDecisionItemService
{
    /// <summary>
    /// 添加候选项
    /// </summary>
    Task<bool> AddAsync(DecisionItemDto dto);

    /// <summary>
    /// 删除候选项
    /// </summary>
    Task<bool> DeleteByIdAsync(long id);

    /// <summary>
    /// 更新候选项
    /// </summary>
    Task<bool> UpdateAsync(DecisionItemUpdateDto dto);

    /// <summary>
    /// 更新状态
    /// </summary>
    Task<bool> UpdateStateAsync(long id, CommonState state);

    /// <summary>
    /// 获取当前用户指定类型的候选项
    /// </summary>
    Task<List<DecisionItemDto>> GetByUserIdAsync(DecisionItemType type);

    /// <summary>
    /// 随机抽取候选项
    /// </summary>
    Task<List<DecisionItemDto>> DrawAsync(DecisionDrawReqDto dto);
}
