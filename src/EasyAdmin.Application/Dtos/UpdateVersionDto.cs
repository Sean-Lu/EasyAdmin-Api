using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 版本列表DTO
/// </summary>
public class UpdateVersionDto : TenantDtoBase
{
    public string AppCode { get; set; }
    public string VersionName { get; set; }
    public int VersionCode { get; set; }
    public string Platform { get; set; }
    public string? Changelog { get; set; }
    public bool IsForceUpdate { get; set; }
    public int MinSupportedVersionCode { get; set; }
    public CommonState State { get; set; }
    public DateTime? PublishTime { get; set; }
    /// <summary>版本包含的文件数量（服务层动态汇总）</summary>
    public int FileCount { get; set; }
    /// <summary>版本更新包总大小（服务层动态汇总，字节）</summary>
    public long TotalSize { get; set; }
}

/// <summary>
/// 版本分页查询请求：支持按版本号名称模糊搜索、按平台筛选
/// </summary>
public class UpdateVersionPageReqDto : PageRequestBase
{
    public string? AppCode { get; set; }

    public string? VersionName { get; set; }

    public string? Platform { get; set; }
}

/// <summary>
/// 版本注册请求：上传zip包创建新版本
/// - UpdateType=Full 时上传完整zip包
/// - UpdateType=Incremental 时上传仅含变更文件的zip，未变文件自动从最新已发布版本继承
/// </summary>
public class UpdateVersionAddDto
{
    public string AppCode { get; set; }

    public string VersionName { get; set; }

    public string Platform { get; set; }
    public string? Changelog { get; set; }
    public bool IsForceUpdate { get; set; }
    public int MinSupportedVersionCode { get; set; }
    /// <summary>更新包类型：Full=全量包, Incremental=增量包</summary>
    public UpdatePackageType UpdateType { get; set; } = UpdatePackageType.Full;
}

/// <summary>
/// 版本信息更新请求：可修改版本名称、更新日志、强制更新标志等元数据
/// </summary>
public class UpdateVersionUpdateDto
{
    public long Id { get; set; }
    public string VersionName { get; set; }
    public string? Changelog { get; set; }
    public bool IsForceUpdate { get; set; }
    public int MinSupportedVersionCode { get; set; }
}

/// <summary>
/// 客户端版本检测结果：返回给客户端的检查结果
/// ForceUpdate为true时客户端必须更新（不能选择"稍后提醒"）
/// </summary>
public class UpdateCheckResultDto
{
    public bool HasUpdate { get; set; }
    public string LatestVersionName { get; set; }
    public int LatestVersionCode { get; set; }
    public string? Changelog { get; set; }
    /// <summary>强制更新：true=客户端不能跳过, false=可选</summary>
    public bool ForceUpdate { get; set; }
    public DateTime? PublishTime { get; set; }
}

/// <summary>
/// 增量更新清单结果：服务端对比两个版本后返回的差异文件列表
/// FilesToDownload 包含 add（新增）和 update（更新）两种操作
/// FilesToDelete 包含 delete（删除）操作
/// </summary>
public class UpdateManifestResultDto
{
    public string TargetVersionName { get; set; }
    public int TargetVersionCode { get; set; }
    public long TotalDownloadSize { get; set; }
    public List<FileEntryDto> FilesToDownload { get; set; } = new();
    public List<FileEntryDto> FilesToDelete { get; set; } = new();
}

/// <summary>
/// 文件条目DTO：描述更新清单中的一个文件及其操作
/// Action: add(新增), update(更新), delete(删除)
/// </summary>
public class FileEntryDto
{
    /// <summary>文件在程序目录下的相对路径（如 bin/Core.dll）</summary>
    public string FilePath { get; set; }
    /// <summary>文件字节大小</summary>
    public long FileSize { get; set; }
    /// <summary>文件SHA256校验和</summary>
    public string Checksum { get; set; }
    /// <summary>操作类型：add / update / delete</summary>
    public string Action { get; set; }
}

/// <summary>
/// 版本详情DTO：在列表DTO基础上增加完整文件清单
/// </summary>
public class UpdateVersionDetailDto : UpdateVersionDto
{
    /// <summary>版本包含的全部文件</summary>
    public List<FileEntryDto> Files { get; set; } = new();
}