using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.SeedData;

/// <summary>
/// 部门种子数据
/// </summary>
public class DepartmentSeedData : IEntitySeedData<DepartmentEntity>
{
    public IEnumerable<DepartmentEntity> SeedData()
    {
        return new[]
        {
            new DepartmentEntity
            {
                Id = 1000,
                PId = 0,
                TenantId = SysConst.DefaultTenantId,
                Name = "技术部",
                Sort = 1,
                State = CommonState.Enable,
                Remark = "技术研发部门"
            },
            new DepartmentEntity
            {
                Id = 1001,
                PId = 1000,
                TenantId = SysConst.DefaultTenantId,
                Name = "前端开发组",
                Sort = 1,
                State = CommonState.Enable,
                Remark = "前端技术开发"
            },
            new DepartmentEntity
            {
                Id = 1002,
                PId = 1000,
                TenantId = SysConst.DefaultTenantId,
                Name = "后端开发组",
                Sort = 2,
                State = CommonState.Enable,
                Remark = "后端技术开发"
            },
            new DepartmentEntity
            {
                Id = 1003,
                PId = 1000,
                TenantId = SysConst.DefaultTenantId,
                Name = "测试组",
                Sort = 3,
                State = CommonState.Enable,
                Remark = "软件测试"
            },
            new DepartmentEntity
            {
                Id = 2000,
                PId = 0,
                TenantId = SysConst.DefaultTenantId,
                Name = "产品部",
                Sort = 2,
                State = CommonState.Enable,
                Remark = "产品设计部门"
            },
            new DepartmentEntity
            {
                Id = 3000,
                PId = 0,
                TenantId = SysConst.DefaultTenantId,
                Name = "运营部",
                Sort = 3,
                State = CommonState.Enable,
                Remark = "运营推广部门"
            },
            new DepartmentEntity
            {
                Id = 4000,
                PId = 0,
                TenantId = SysConst.DefaultTenantId,
                Name = "销售部",
                Sort = 4,
                State = CommonState.Enable,
                Remark = "销售与市场"
            },
            new DepartmentEntity
            {
                Id = 5000,
                PId = 0,
                TenantId = SysConst.DefaultTenantId,
                Name = "人事行政部",
                Sort = 5,
                State = CommonState.Enable,
                Remark = "人事与行政"
            },
            new DepartmentEntity
            {
                Id = 6000,
                PId = 0,
                TenantId = SysConst.DefaultTenantId,
                Name = "财务部",
                Sort = 6,
                State = CommonState.Enable,
                Remark = "财务与会计"
            }
        };
    }
}