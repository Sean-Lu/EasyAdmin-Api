using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 版本管理服务接口：定义客户端自动更新的全部业务操作
/// 包含版本检测、更新清单生成、文件下载、版本注册、发布管理等
/// </summary>
public interface IVersionService
{
    /// <summary>
    /// 客户端版本检测：比较当前版本码与服务端已发布最新版本
    /// </summary>
    Task<UpdateCheckResultDto> CheckAsync(int currentVersionCode, string appCode, string platform);

    /// <summary>
    /// 获取指定应用+平台的最新已发布版本信息
    /// </summary>
    Task<UpdateCheckResultDto?> GetLatestAsync(string appCode, string platform);

    /// <summary>
    /// 获取增量更新清单：直接对比当前版本与目标版本，跨版本跳过中间迭代
    /// </summary>
    Task<UpdateManifestResultDto> GetManifestAsync(int currentVersionCode, int targetVersionCode, string appCode, string platform);

    /// <summary>
    /// 获取单个更新文件流，供客户端按需下载（含校验和）
    /// </summary>
    Task<(Stream Stream, string Checksum, string FileName)> GetFileStreamAsync(int versionCode, string filePath, string appCode, string platform);

    /// <summary>
    /// 动态生成版本完整zip包并返回流
    /// </summary>
    Task<(Stream ZipStream, string VersionName)> DownloadZipAsync(long versionId);

    /// <summary>
    /// 注册新版本：解压上传zip包，计算SHA256，存储文件，写入数据库
    /// </summary>
    Task<long> RegisterAsync(UpdateVersionAddDto dto, string zipFilePath);

    /// <summary>
    /// 分页查询版本列表
    /// </summary>
    Task<PageQueryResult<UpdateVersionDto>> PageAsync(UpdateVersionPageReqDto request);

    /// <summary>
    /// 查看版本详情：含完整文件列表
    /// </summary>
    Task<UpdateVersionDetailDto> DetailAsync(long id);

    /// <summary>
    /// 更新版本元数据
    /// </summary>
    Task<bool> UpdateAsync(UpdateVersionUpdateDto dto);

    /// <summary>
    /// 发布/回滚版本：启用时自动禁用同平台其他已发布版本
    /// </summary>
    Task<bool> UpdateStateAsync(long id, CommonState state);

    /// <summary>
    /// 删除版本及关联的所有文件和存储对象（已发布版本不能删除）
    /// </summary>
    Task<bool> DeleteAsync(long id);
}