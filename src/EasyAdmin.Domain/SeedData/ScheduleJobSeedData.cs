using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.SeedData;

/// <summary>
/// 定时任务种子数据
/// </summary>
public class ScheduleJobSeedData : IEntitySeedData<ScheduleJobEntity>
{
    public IEnumerable<ScheduleJobEntity> SeedData()
    {
        return new[]
        {
            new ScheduleJobEntity
            {
                Id = 1,
                TenantId = SysConst.DefaultTenantId,
                JobName = "测试任务",
                ScheduleType = ScheduleType.Simple,
                SimpleInterval = 10,
                SimpleIntervalUnit = SimpleIntervalUnit.Second,
                JobClassName = "EasyAdmin.Application.Jobs.TestJob",
                JobData = "{\"Message\": \"自定义消息\", \"Count\": 3}",
                Description = "测试任务，每10秒执行一次，仅供示例使用",
                State = CommonState.Disable
            }
        };
    }
}