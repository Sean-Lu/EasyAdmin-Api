using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
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
            new MenuEntity{ Id = 3000002, PId = 3000000, Sort = 2, Icon = "AppstoreOutlined", Title = "使用 Component", Path = "/proTable/useComponent", State = CommonState.Disable },

            new MenuEntity{ Id = 4000000, PId = 0, Sort = 4, Icon = "FundOutlined", Title = "Dashboard", Path = "/dashboard", State = CommonState.Disable },
            new MenuEntity{ Id = 4000001, PId = 4000000, Sort = 1, Icon = "AppstoreOutlined", Title = "数据可视化", Path = "/dashboard/dataVisualize", State = CommonState.Disable },
            new MenuEntity{ Id = 4000002, PId = 4000000, Sort = 2, Icon = "AppstoreOutlined", Title = "内嵌页面", Path = "/dashboard/embedded", State = CommonState.Disable },

            new MenuEntity{ Id = 5000000, PId = 0, Sort = 5, Icon = "FileTextOutlined", Title = "表单 Form", Path = "/form", State = CommonState.Disable },
            new MenuEntity{ Id = 5000001, PId = 5000000, Sort = 1, Icon = "AppstoreOutlined", Title = "基础 Form", Path = "/form/basicForm", State = CommonState.Disable },
            new MenuEntity{ Id = 5000002, PId = 5000000, Sort = 2, Icon = "AppstoreOutlined", Title = "校验 Form", Path = "/form/validateForm", State = CommonState.Disable },
            new MenuEntity{ Id = 5000003, PId = 5000000, Sort = 3, Icon = "AppstoreOutlined", Title = "动态 Form", Path = "/form/dynamicForm", State = CommonState.Disable },

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
            new MenuEntity{ Id = 7000003, PId = 7000000, Sort = 3, Icon = "AppstoreOutlined", Title = "Icon 选择", Path = "/assembly/selectIcon", State = CommonState.Disable },
            new MenuEntity{ Id = 7000004, PId = 7000000, Sort = 4, Icon = "AppstoreOutlined", Title = "批量导入数据", Path = "/assembly/batchImport", State = CommonState.Disable },

            //new MenuEntity{ Id = 8000000, PId = 0, Sort = 8, Icon = "ProfileOutlined", Title = "菜单嵌套", Path = "/menu", State = CommonState.Disable },
            //new MenuEntity{ Id = 8000001, PId = 8000000, Sort = 1, Icon = "AppstoreOutlined", Title = "菜单1", Path = "/menu/menu1", State = CommonState.Disable },
            //new MenuEntity{ Id = 8000002, PId = 8000000, Sort = 2, Icon = "AppstoreOutlined", Title = "菜单2", Path = "/menu/menu2", State = CommonState.Disable },
            //new MenuEntity{ Id = 8000100, PId = 8000002, Sort = 3, Icon = "AppstoreOutlined", Title = "菜单2-1", Path = "/menu/menu2/menu21", State = CommonState.Disable },
            //new MenuEntity{ Id = 8000200, PId = 8000002, Sort = 4, Icon = "AppstoreOutlined", Title = "菜单2-2", Path = "/menu/menu2/menu22", State = CommonState.Disable },
            //new MenuEntity{ Id = 8010000, PId = 8000200, Sort = 5, Icon = "AppstoreOutlined", Title = "菜单2-2-1", Path = "/menu/menu2/menu22/menu221", State = CommonState.Disable },
            //new MenuEntity{ Id = 8020000, PId = 8000200, Sort = 6, Icon = "AppstoreOutlined", Title = "菜单2-2-2", Path = "/menu/menu2/menu22/menu222", State = CommonState.Disable },
            //new MenuEntity{ Id = 8000300, PId = 8000002, Sort = 7, Icon = "AppstoreOutlined", Title = "菜单2-3", Path = "/menu/menu2/menu23", State = CommonState.Disable },
            //new MenuEntity{ Id = 8000003, PId = 8000000, Sort = 8, Icon = "AppstoreOutlined", Title = "菜单3", Path = "/menu/menu3", State = CommonState.Disable },

            new MenuEntity{ Id = 8000000, PId = 0, Sort = 9, Icon = "SettingOutlined", Title = "系统管理", Path = "/system", State = CommonState.Enable },
            new MenuEntity{ Id = 8000001, PId = 8000000, Sort = 1, Icon = "ShopOutlined", Title = "租户管理", Path = "/system/tenant", State = CommonState.Enable },
            new MenuEntity{ Id = 8000002, PId = 8000000, Sort = 2, Icon = "UserOutlined", Title = "用户管理", Path = "/system/user", State = CommonState.Enable },
            new MenuEntity{ Id = 8000003, PId = 8000000, Sort = 3, Icon = "SolutionOutlined", Title = "角色管理", Path = "/system/role", State = CommonState.Enable },
            new MenuEntity{ Id = 8000004, PId = 8000000, Sort = 4, Icon = "AppstoreOutlined", Title = "部门管理", Path = "/system/dept", State = CommonState.Enable },
            new MenuEntity{ Id = 8000005, PId = 8000000, Sort = 5, Icon = "AppstoreOutlined", Title = "岗位管理", Path = "/system/post", State = CommonState.Enable },
            new MenuEntity{ Id = 8000006, PId = 8000000, Sort = 6, Icon = "MenuOutlined", Title = "菜单管理", Path = "/system/menu", State = CommonState.Enable },
            new MenuEntity{ Id = 8000007, PId = 8000000, Sort = 7, Icon = "AppstoreOutlined", Title = "字典管理", Path = "/system/dict", State = CommonState.Enable },
            new MenuEntity{ Id = 8000008, PId = 8000000, Sort = 8, Icon = "AppstoreOutlined", Title = "参数管理", Path = "/system/param", State = CommonState.Enable },
            new MenuEntity{ Id = 8000009, PId = 8000000, Sort = 9, Icon = "AppstoreOutlined", Title = "应用管理", Path = "/system/client", State = CommonState.Disable },
            new MenuEntity{ Id = 8000010, PId = 8000000, Sort = 10, Icon = "AppstoreOutlined", Title = "缓存管理", Path = "/system/cache", State = CommonState.Disable },
            new MenuEntity{ Id = 8000011, PId = 8000000, Sort = 11, Icon = "FieldTimeOutlined", Title = "定时任务", Path = "/system/job", State = CommonState.Disable },
            new MenuEntity{ Id = 8000013, PId = 8000000, Sort = 13, Icon = "UnorderedListOutlined", Title = "任务管理", Path = "/system/task", State = CommonState.Enable },
            new MenuEntity{ Id = 8000014, PId = 8000000, Sort = 14, Icon = "FileDoneOutlined", Title = "文件管理", Path = "/system/file", State = CommonState.Enable },

            new MenuEntity{ Id = 9000000, PId = 0, Sort = 10, Icon = "ExclamationCircleOutlined", Title = "错误页面", Path = "/error", State = CommonState.Disable },
            new MenuEntity{ Id = 9000001, PId = 9000000, Sort = 1, Icon = "AppstoreOutlined", Title = "404页面", Path = "/404", State = CommonState.Disable },
            new MenuEntity{ Id = 9000002, PId = 9000000, Sort = 2, Icon = "AppstoreOutlined", Title = "403页面", Path = "/403", State = CommonState.Disable },
            new MenuEntity{ Id = 9000003, PId = 9000000, Sort = 3, Icon = "AppstoreOutlined", Title = "500页面", Path = "/500", State = CommonState.Disable },

            new MenuEntity{ Id = 10000000, PId = 0, Sort = 11, Icon = "LinkOutlined", Title = "外部链接", Path = "/link", State = CommonState.Enable },
            new MenuEntity{ Id = 10000001, PId = 10000000, Sort = 1, Icon = "LinkOutlined", Title = "Gitee 仓库", Path = "/link/gitee", OutLink = "https://gitee.com/Sean-Lu", State = CommonState.Enable },
            new MenuEntity{ Id = 10000002, PId = 10000000, Sort = 2, Icon = "LinkOutlined", Title = "GitHub 仓库", Path = "/link/github", OutLink = "https://github.com/Sean-Lu", State = CommonState.Enable },
            new MenuEntity{ Id = 10000003, PId = 10000000, Sort = 3, Icon = "LinkOutlined", Title = "掘金文档", Path = "/link/juejin", OutLink = "https://juejin.cn/", State = CommonState.Disable },
            new MenuEntity{ Id = 10000004, PId = 10000000, Sort = 4, Icon = "LinkOutlined", Title = "个人博客", Path = "/link/myBlog", OutLink = "https://www.cnblogs.com/", State = CommonState.Disable },

            new MenuEntity{ Id = 11000000, PId = 0, Sort = 12, Icon = "ToolOutlined", Title = "工具", Path = "/tool", State = CommonState.Enable },
            new MenuEntity{ Id = 11000001, PId = 11000000, Sort = 1, Icon = "OrderedListOutlined", Title = "待办事项", Path = "/tool/todoList", State = CommonState.Enable },// To-do List
            new MenuEntity{ Id = 11000002, PId = 11000000, Sort = 2, Icon = "CheckCircleOutlined", Title = "签到", Path = "/tool/checkIn", State = CommonState.Enable },
            new MenuEntity{ Id = 11000003, PId = 11000000, Sort = 3, Icon = "FormOutlined", Title = "日报", Path = "/tool/dayWorkReport", State = CommonState.Enable },
            new MenuEntity{ Id = 11000004, PId = 11000000, Sort = 4, Icon = "FormOutlined", Title = "周报", Path = "/tool/weekWorkReport", State = CommonState.Disable },
            new MenuEntity{ Id = 11000005, PId = 11000000, Sort = 5, Icon = "FormOutlined", Title = "月报", Path = "/tool/monthWorkReport", State = CommonState.Disable },
            new MenuEntity{ Id = 11000006, PId = 11000000, Sort = 6, Icon = "KeyOutlined", Title = "加解密", Path = "/tool/crypto", State = CommonState.Enable },

            new MenuEntity{ Id = 12000000, PId = 0, Sort = 12, Icon = "ProfileOutlined", Title = "日志管理", Path = "/log", State = CommonState.Enable },
            new MenuEntity{ Id = 12000001, PId = 12000000, Sort = 1, Icon = "AppstoreOutlined", Title = "登录日志", Path = "/log/loginLog", State = CommonState.Enable },
            new MenuEntity{ Id = 12000002, PId = 12000000, Sort = 2, Icon = "AppstoreOutlined", Title = "操作日志", Path = "/log/operateLog", State = CommonState.Enable },
        };
    }
}