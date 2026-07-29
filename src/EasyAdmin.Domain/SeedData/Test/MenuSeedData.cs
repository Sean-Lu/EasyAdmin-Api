using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.SeedData.Test;

/// <summary>
/// 测试 - 菜单种子数据
/// </summary>
public class MenuSeedData : IEntitySeedData<MenuEntity>, ITestSeedData
{
    /// <inheritdoc />
    public IEnumerable<MenuEntity> SeedData()
    {
        return new[]
        {
            new MenuEntity{ Type = MenuType.Directory, Id = 3000000, PId = 0, Sort = 3, Icon = "TableOutlined", Title = "超级表格", State = CommonState.Disable },
            new MenuEntity{ Type = MenuType.Internal, Id = 3000001, PId = 3000000, Sort = 1, Icon = "AppstoreOutlined", Title = "使用 Hooks", Path = "/proTable/useHooks", State = CommonState.Disable },

            new MenuEntity{ Type = MenuType.Directory, Id = 4000000, PId = 0, Sort = 4, Icon = "FundOutlined", Title = "Dashboard", State = CommonState.Disable },
            new MenuEntity{ Type = MenuType.Internal, Id = 4000001, PId = 4000000, Sort = 1, Icon = "AppstoreOutlined", Title = "数据可视化", Path = "/dashboard/dataVisualize", State = CommonState.Disable },
            new MenuEntity{ Type = MenuType.Internal, Id = 4000002, PId = 4000000, Sort = 2, Icon = "AppstoreOutlined", Title = "内嵌页面", Path = "/dashboard/embedded", State = CommonState.Disable },

            new MenuEntity{ Type = MenuType.Directory, Id = 6000000, PId = 0, Sort = 6, Icon = "PieChartOutlined", Title = "Echarts图表", State = CommonState.Disable },
            new MenuEntity{ Type = MenuType.Internal, Id = 6000001, PId = 6000000, Sort = 1, Icon = "AppstoreOutlined", Title = "水型图", Path = "/echarts/waterChart", State = CommonState.Disable },
            new MenuEntity{ Type = MenuType.Internal, Id = 6000002, PId = 6000000, Sort = 2, Icon = "AppstoreOutlined", Title = "柱状图", Path = "/echarts/columnChart", State = CommonState.Disable },
            new MenuEntity{ Type = MenuType.Internal, Id = 6000003, PId = 6000000, Sort = 3, Icon = "AppstoreOutlined", Title = "折线图", Path = "/echarts/lineChart", State = CommonState.Disable },
            new MenuEntity{ Type = MenuType.Internal, Id = 6000004, PId = 6000000, Sort = 4, Icon = "AppstoreOutlined", Title = "饼图", Path = "/echarts/pieChart", State = CommonState.Disable },
            new MenuEntity{ Type = MenuType.Internal, Id = 6000005, PId = 6000000, Sort = 5, Icon = "AppstoreOutlined", Title = "雷达图", Path = "/echarts/radarChart", State = CommonState.Disable },
            new MenuEntity{ Type = MenuType.Internal, Id = 6000006, PId = 6000000, Sort = 6, Icon = "AppstoreOutlined", Title = "嵌套环形图", Path = "/echarts/nestedChart", State = CommonState.Disable },

            new MenuEntity{ Type = MenuType.Directory, Id = 7000000, PId = 0, Sort = 7, Icon = "ShoppingOutlined", Title = "常用组件", State = CommonState.Disable },
            new MenuEntity{ Type = MenuType.Internal, Id = 7000001, PId = 7000000, Sort = 1, Icon = "AppstoreOutlined", Title = "引导页", Path = "/assembly/guide", State = CommonState.Disable },
            new MenuEntity{ Type = MenuType.Internal, Id = 7000002, PId = 7000000, Sort = 2, Icon = "AppstoreOutlined", Title = "Svg 图标", Path = "/assembly/svgIcon", State = CommonState.Disable },

            new MenuEntity{ Type = MenuType.Directory, Id = 9000000, PId = 0, Sort = 10, Icon = "ExclamationCircleOutlined", Title = "错误页面", State = CommonState.Disable },
            new MenuEntity{ Type = MenuType.Internal, Id = 9000001, PId = 9000000, Sort = 1, Icon = "AppstoreOutlined", Title = "404页面", Path = "/404", State = CommonState.Disable },
            new MenuEntity{ Type = MenuType.Internal, Id = 9000002, PId = 9000000, Sort = 2, Icon = "AppstoreOutlined", Title = "403页面", Path = "/403", State = CommonState.Disable },
            new MenuEntity{ Type = MenuType.Internal, Id = 9000003, PId = 9000000, Sort = 3, Icon = "AppstoreOutlined", Title = "500页面", Path = "/500", State = CommonState.Disable },
        };
    }
}