using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

public interface INoteCategoryService
{
    Task<bool> AddAsync(NoteCategoryDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> UpdateAsync(NoteCategoryUpdateDto dto);
    Task<bool> UpdateSortOrderAsync(long id, int sortOrder);
    Task<List<NoteCategoryDto>> GetByUserIdAsync();
}
