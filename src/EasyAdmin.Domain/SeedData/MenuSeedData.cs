using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.SeedData;

/// <summary>
/// 菜单种子数据
/// </summary>
public class MenuSeedData : IEntitySeedData<MenuEntity>
{
    public IEnumerable<MenuEntity> SeedData()
    {
        return new[]
        {
            new MenuEntity{ Id = 1000000, PId = 0, Sort = 1, Icon = "HomeOutlined", Title = "首页", Path = "/home/index", State = CommonState.Enable },

            new MenuEntity{ Id = 2000000, PId = 0, Sort = 2, Icon = "AreaChartOutlined", Title = "数据大屏", Path = "/dataScreen/index", State = CommonState.Enable },

            new MenuEntity{ Id = 3000000, PId = 0, Sort = 3, Icon = "TableOutlined", Title = "超级表格", Path = "/proTable", State = CommonState.Disable },
            new MenuEntity{ Id = 3000001, PId = 3000000, Sort = 1, Icon = "AppstoreOutlined", Title = "使用 Hooks", Path = "/proTable/useHooks", State = CommonState.Disable },

            new MenuEntity{ Id = 4000000, PId = 0, Sort = 4, Icon = "FundOutlined", Title = "Dashboard", Path = "/dashboard", State = CommonState.Disable },
            new MenuEntity{ Id = 4000001, PId = 4000000, Sort = 1, Icon = "AppstoreOutlined", Title = "数据可视化", Path = "/dashboard/dataVisualize", State = CommonState.Disable },
            new MenuEntity{ Id = 4000002, PId = 4000000, Sort = 2, Icon = "AppstoreOutlined", Title = "内嵌页面", Path = "/dashboard/embedded", State = CommonState.Disable },

            new MenuEntity{ Id = 6000000, PId = 0, Sort = 6, Icon = "PieChartOutlined", Title = "Echarts图表", Path = "/echarts", State = CommonState.Disable },
            new MenuEntity{ Id = 6000001, PId = 6000000, Sort = 1, Icon = "AppstoreOutlined", Title = "水型图", Path = "/echarts/waterChart", State = CommonState.Disable },
            new MenuEntity{ Id = 6000002, PId = 6000000, Sort = 2, Icon = "AppstoreOutlined", Title = "柱状图", Path = "/echarts/columnChart", State = CommonState.Disable },
            new MenuEntity{ Id = 6000003, PId = 6000000, Sort = 3, Icon = "AppstoreOutlined", Title = "折线图", Path = "/echarts/lineChart", State = CommonState.Disable },
            new MenuEntity{ Id = 6000004, PId = 6000000, Sort = 4, Icon = "AppstoreOutlined", Title = "饼图", Path = "/echarts/pieChart", State = CommonState.Disable },
            new MenuEntity{ Id = 6000005, PId = 6000000, Sort = 5, Icon = "AppstoreOutlined", Title = "雷达图", Path = "/echarts/radarChart", State = CommonState.Disable },
            new MenuEntity{ Id = 6000006, PId = 6000000, Sort = 6, Icon = "AppstoreOutlined", Title = "嵌套环形图", Path = "/echarts/nestedChart", State = CommonState.Disable },

            new MenuEntity{ Id = 7000000, PId = 0, Sort = 7, Icon = "ShoppingOutlined", Title = "常用组件", Path = "/assembly", State = CommonState.Disable },
            new MenuEntity{ Id = 7000001, PId = 7000000, Sort = 1, Icon = "AppstoreOutlined", Title = "引导页", Path = "/assembly/guide", State = CommonState.Disable },
            new MenuEntity{ Id = 7000002, PId = 7000000, Sort = 2, Icon = "AppstoreOutlined", Title = "Svg 图标", Path = "/assembly/svgIcon", State = CommonState.Disable },

            new MenuEntity{ Id = 8000000, PId = 0, Sort = 8, Icon = "SettingOutlined", Title = "系统管理", Path = "/system", State = CommonState.Enable },
            new MenuEntity{ Id = SysConst.TenantMenuId, PId = 8000000, Sort = 1, Icon = "ShopOutlined", Title = "租户管理", Path = "/system/tenant", State = CommonState.Enable },
            new MenuEntity{ Id = 8000002, PId = 8000000, Sort = 2, Icon = "UserOutlined", Title = "用户管理", Path = "/system/user", State = CommonState.Enable },
            new MenuEntity{ Id = 8000003, PId = 8000000, Sort = 3, Icon = "SolutionOutlined", Title = "角色管理", Path = "/system/role", State = CommonState.Enable },
            new MenuEntity{ Id = 8000004, PId = 8000000, Sort = 4, Icon = "AppstoreOutlined", Title = "部门管理", Path = "/system/department", State = CommonState.Enable },
            new MenuEntity{ Id = 8000005, PId = 8000000, Sort = 5, Icon = "AppstoreOutlined", Title = "岗位管理", Path = "/system/position", State = CommonState.Enable },
            new MenuEntity{ Id = 8000006, PId = 8000000, Sort = 6, Icon = "EnvironmentOutlined", Title = "行政区划", Path = "/system/region", State = CommonState.Enable },
            new MenuEntity{ Id = 8000007, PId = 8000000, Sort = 7, Icon = "MenuOutlined", Title = "菜单管理", Path = "/system/menu", State = CommonState.Enable },
            new MenuEntity{ Id = 8000008, PId = 8000000, Sort = 8, Icon = "AppstoreOutlined", Title = "字典管理", Path = "/system/dict", State = CommonState.Enable },
            new MenuEntity{ Id = 8000009, PId = 8000000, Sort = 9, Icon = "AppstoreOutlined", Title = "参数管理", Path = "/system/param", State = CommonState.Enable },
            new MenuEntity{ Id = 8000010, PId = 8000000, Sort = 10, Icon = "AppstoreOutlined", Title = "应用管理", Path = "/system/client", State = CommonState.Disable },
            new MenuEntity{ Id = 8000011, PId = 8000000, Sort = 11, Icon = "DatabaseOutlined", Title = "缓存管理", Path = "/system/cache", State = CommonState.Enable },
            new MenuEntity{ Id = 8000012, PId = 8000000, Sort = 12, Icon = "FieldTimeOutlined", Title = "定时任务", Path = "/system/job", State = CommonState.Enable },
            new MenuEntity{ Id = 8000013, PId = 8000000, Sort = 13, Icon = "FileDoneOutlined", Title = "文件管理", Path = "/system/file", State = CommonState.Enable },
            new MenuEntity{ Id = 8000014, PId = 8000000, Sort = 14, Icon = "CloudUploadOutlined", Title = "更新管理", Path = "/system/update", State = CommonState.Enable },
            new MenuEntity{ Id = 8000015, PId = 8000000, Sort = 15, Icon = "BellOutlined", Title = "通知管理", Path = "/system/notification", State = CommonState.Enable },

            new MenuEntity{ Id = 8100000, PId = 0, Sort = 9, Icon = "UserOutlined", Title = "个人中心", Path = "/user", State = CommonState.Enable },
            new MenuEntity{ Id = 8100001, PId = 8100000, Sort = 1, Icon = "BellOutlined", Title = "我的消息", Path = "/user/message", State = CommonState.Enable },
            new MenuEntity{ Id = 8100002, PId = 8100000, Sort = 2, Icon = "BookOutlined", Title = "我的笔记", Path = "/user/note", State = CommonState.Enable },
            new MenuEntity{ Id = 8100003, PId = 8100000, Sort = 3, Icon = "OrderedListOutlined", Title = "待办事项", Path = "/user/todoList", State = CommonState.Enable },// To-do List
            new MenuEntity{ Id = 8100004, PId = 8100000, Sort = 4, Icon = "CheckCircleOutlined", Title = "签到", Path = "/user/checkIn", State = CommonState.Enable },
            new MenuEntity{ Id = 8100005, PId = 8100000, Sort = 5, Icon = "FormOutlined", Title = "日报", Path = "/user/dayWorkReport", State = CommonState.Enable },
            new MenuEntity{ Id = 8100006, PId = 8100000, Sort = 6, Icon = "FormOutlined", Title = "周报", Path = "/user/weekWorkReport", State = CommonState.Enable },
            new MenuEntity{ Id = 8100007, PId = 8100000, Sort = 7, Icon = "FormOutlined", Title = "月报", Path = "/user/monthWorkReport", State = CommonState.Enable },

            new MenuEntity{ Id = 9000000, PId = 0, Sort = 10, Icon = "ExclamationCircleOutlined", Title = "错误页面", Path = "/error", State = CommonState.Disable },
            new MenuEntity{ Id = 9000001, PId = 9000000, Sort = 1, Icon = "AppstoreOutlined", Title = "404页面", Path = "/404", State = CommonState.Disable },
            new MenuEntity{ Id = 9000002, PId = 9000000, Sort = 2, Icon = "AppstoreOutlined", Title = "403页面", Path = "/403", State = CommonState.Disable },
            new MenuEntity{ Id = 9000003, PId = 9000000, Sort = 3, Icon = "AppstoreOutlined", Title = "500页面", Path = "/500", State = CommonState.Disable },

            new MenuEntity{ Id = 10000000, PId = 0, Sort = 11, Icon = "LinkOutlined", Title = "外部链接", Path = "/link", State = CommonState.Enable },
            new MenuEntity{ Id = 10000001, PId = 10000000, Sort = 1, Icon = "LinkOutlined", Title = "Gitee 仓库", Path = "/link/gitee", OutLink = "https://gitee.com/Sean-Lu", OutLinkOpenType = OutLinkOpenType.Blank, State = CommonState.Enable },
            new MenuEntity{ Id = 10000002, PId = 10000000, Sort = 2, Icon = "LinkOutlined", Title = "GitHub 仓库", Path = "/link/github", OutLink = "https://github.com/Sean-Lu", OutLinkOpenType = OutLinkOpenType.Blank, State = CommonState.Enable },
            new MenuEntity{ Id = 10000003, PId = 10000000, Sort = 3, Icon = "SearchOutlined", Title = "百度搜索", Path = "/link/baidu", OutLink = "https://www.baidu.com", OutLinkOpenType = OutLinkOpenType.Inline, State = CommonState.Enable },

            new MenuEntity{ Id = 11000000, PId = 0, Sort = 12, Icon = "ToolOutlined", Title = "工具", Path = "/tool", State = CommonState.Enable },
            new MenuEntity{ Id = 11000002, PId = 11000000, Sort = 1, Icon = "CodeOutlined", Title = "代码生成", Path = "/tool/codeGen", State = CommonState.Enable },
            new MenuEntity{ Id = 11000003, PId = 11000000, Sort = 2, Icon = "GiftOutlined", Title = "百宝箱", Path = "/tool/commonTools", State = CommonState.Enable },

            new MenuEntity{ Id = 12000000, PId = 0, Sort = 13, Icon = "ProfileOutlined", Title = "日志管理", Path = "/log", State = CommonState.Enable },
            new MenuEntity{ Id = 12000001, PId = 12000000, Sort = 1, Icon = "AppstoreOutlined", Title = "登录日志", Path = "/log/loginLog", State = CommonState.Enable },
            new MenuEntity{ Id = 12000002, PId = 12000000, Sort = 2, Icon = "AppstoreOutlined", Title = "操作日志", Path = "/log/operateLog", State = CommonState.Enable },

            new MenuEntity{ Id = 13000000, PId = 0, Sort = 14, Icon = "MonitorOutlined", Title = "运行监控", Path = "/monitor", State = CommonState.Enable },
            new MenuEntity{ Id = 13000001, PId = 13000000, Sort = 1, Icon = "DashboardOutlined", Title = "服务器监控", Path = "/monitor/server", State = CommonState.Enable },
        };
    }
}