namespace EasyAdmin.Application.Dtos;

public class DepartmentListReqDto
{
    /// <summary>
    /// 是否查询所有部门（包含被禁用的）
    /// </summary>
    public bool All { get; set; }
    /// <summary>
    /// 是否包含顶级部门
    /// </summary>
    public bool IncludeTopDepartment { get; set; }

    /// <summary>
    /// 部门名称
    /// </summary>
    public string? Name { get; set; }
}