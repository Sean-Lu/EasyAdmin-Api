using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 代码生成分类服务接口
/// </summary>
public interface ICodeGenCategoryService
{
    Task<List<CodeGenCategoryDto>> GetTreeAsync();
    Task<List<CodeGenCategoryDto>> GetListAsync();
    Task<CodeGenCategoryDto> GetByIdAsync(long id);
    Task<long> AddAsync(CodeGenCategoryAddDto dto);
    Task<bool> UpdateAsync(CodeGenCategoryUpdateDto dto);
    Task<bool> DeleteAsync(long id);
    Task<string> ExportAsync();
    Task ImportAsync(CodeGenCategoryImportDto dto);
}