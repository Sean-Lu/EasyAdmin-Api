using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface IFileService
{
    Task<bool> AddAsync(FileDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(FileDto dto);
    Task<PageQueryResult<FileEntity>> PageAsync(FilePageReqDto request);
    Task<FileEntity> GetByIdAsync(long id);
}