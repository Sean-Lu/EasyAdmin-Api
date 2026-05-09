using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

public interface ICodeGenService
{
    Task<List<CodeGenTemplateDto>> GetTemplateListAsync(CodeGenTemplateListReqDto request);
    Task<CodeGenTemplateDto> GetTemplateByIdAsync(long id);
    Task<bool> AddTemplateAsync(CodeGenTemplateDto dto);
    Task<bool> UpdateTemplateAsync(CodeGenTemplateUpdateDto dto);
    Task<bool> DeleteTemplateAsync(long id);
    Task<bool> DeleteTemplatesAsync(List<long> ids);

    Task<List<DbConnectionConfigDto>> GetDbConfigListAsync(DbConnectionConfigListReqDto request);
    Task<DbConnectionConfigDto> GetDbConfigByIdAsync(long id);
    Task<bool> AddDbConfigAsync(DbConnectionConfigUpdateDto dto);
    Task<bool> UpdateDbConfigAsync(DbConnectionConfigUpdateDto dto);
    Task<bool> DeleteDbConfigAsync(long id);
    Task<bool> DeleteDbConfigsAsync(List<long> ids);
    Task<bool> TestDbConnectionAsync(long id);
    Task<List<DbTableInfoDto>> GetDbTablesAsync(long id);

    Task<CodeGenResultDto> GenerateCodeAsync(CodeGenReqDto request);
    Task<byte[]> DownloadFileAsync(string taskId, string fileName);
    Task<byte[]> DownloadPackageAsync(string taskId);
}
