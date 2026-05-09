using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

public interface ICodeGenCategoryService
{
    Task<List<CodeGenCategoryDto>> GetTreeAsync();
    Task<List<CodeGenCategoryDto>> GetListAsync();
    Task<CodeGenCategoryDto> GetByIdAsync(long id);
    Task<long> AddAsync(CodeGenCategoryAddDto request);
    Task UpdateAsync(CodeGenCategoryUpdateDto request);
    Task DeleteAsync(long id);
    Task<string> ExportAsync();
    Task ImportAsync(CodeGenCategoryImportDto request);
}