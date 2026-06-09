using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.SeedData;

/// <summary>
/// 岗位种子数据
/// </summary>
public class PositionSeedData : IEntitySeedData<PositionEntity>
{
    public IEnumerable<PositionEntity> SeedData()
    {
        return new[]
        {
            new PositionEntity 
            { 
                Id = 1, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "全栈开发工程师", 
                Code = "FULL_STACK_DEV", 
                Sort = 1, 
                State = CommonState.Enable, 
                Remark = "全栈软件研发工程师" 
            },
            new PositionEntity 
            { 
                Id = 2, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "前端工程师", 
                Code = "FRONTEND_DEV", 
                Sort = 2, 
                State = CommonState.Enable, 
                Remark = "前端开发工程师" 
            },
            new PositionEntity 
            { 
                Id = 3, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "后端工程师", 
                Code = "BACKEND_DEV", 
                Sort = 3, 
                State = CommonState.Enable, 
                Remark = "后端开发工程师" 
            },
            new PositionEntity 
            { 
                Id = 4, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "测试工程师", 
                Code = "TEST_ENGINEER", 
                Sort = 4, 
                State = CommonState.Enable, 
                Remark = "软件测试工程师" 
            },
            new PositionEntity 
            { 
                Id = 5, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "架构师", 
                Code = "ARCHITECT", 
                Sort = 5, 
                State = CommonState.Enable, 
                Remark = "负责技术架构" 
            },
            new PositionEntity 
            { 
                Id = 6, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "技术总监", 
                Code = "TECH_DIRECTOR", 
                Sort = 6, 
                State = CommonState.Enable, 
                Remark = "技术部门负责人" 
            },
            new PositionEntity 
            { 
                Id = 7, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "产品经理", 
                Code = "PRODUCT_MANAGER", 
                Sort = 7, 
                State = CommonState.Enable, 
                Remark = "产品经理" 
            },
            new PositionEntity 
            { 
                Id = 8, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "UI设计师", 
                Code = "UI_DESIGNER", 
                Sort = 8, 
                State = CommonState.Enable, 
                Remark = "UI界面设计师" 
            },
            new PositionEntity 
            { 
                Id = 9, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "运营专员", 
                Code = "OPERATOR", 
                Sort = 9, 
                State = CommonState.Enable, 
                Remark = "运营专员" 
            },
            new PositionEntity 
            { 
                Id = 10, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "销售专员", 
                Code = "SALES_STAFF", 
                Sort = 10, 
                State = CommonState.Enable, 
                Remark = "销售与市场推广" 
            },
            new PositionEntity 
            { 
                Id = 11, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "人事专员", 
                Code = "HR_STAFF", 
                Sort = 11, 
                State = CommonState.Enable, 
                Remark = "人事专员" 
            },
            new PositionEntity 
            { 
                Id = 12, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "财务专员", 
                Code = "FINANCE_STAFF", 
                Sort = 12, 
                State = CommonState.Enable, 
                Remark = "财务专员" 
            },
            new PositionEntity 
            { 
                Id = 13, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "项目经理", 
                Code = "PROJECT_MANAGER", 
                Sort = 13, 
                State = CommonState.Enable, 
                Remark = "项目管理" 
            }
        };
    }
}