using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using MapsterMapper;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

public class AppCodeService(
    IMapper mapper,
    IAppCodeRepository appCodeRepository
) : IAppCodeService
{
    public async Task<bool> AddAsync(AppCodeAddDto dto)
    {
        var existing = await appCodeRepository.QueryAsync(
            entity => entity.Code == dto.Code && !entity.IsDelete);
        if (existing?.FirstOrDefault() != null)
        {
            throw new InvalidOperationException($"应用标识 {dto.Code} 已存在");
        }

        var entity = mapper.Map<AppCodeEntity>(dto);
        entity.State = CommonState.Enable;
        return await appCodeRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await appCodeRepository.UpdateAsync(new AppCodeEntity
        {
            Id = id,
            IsDelete = true,
            UpdateTime = DateTime.Now
        }, entity => new { entity.IsDelete, entity.UpdateTime }) > 0;
    }

    public async Task<bool> UpdateAsync(AppCodeUpdateDto dto)
    {
        return await appCodeRepository.UpdateByDtoAsync(dto, mapper.Map<AppCodeEntity>) > 0;
    }

    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await appCodeRepository.UpdateAsync(new AppCodeEntity
        {
            Id = id,
            State = state,
            UpdateTime = DateTime.Now
        }, entity => new { entity.State, entity.UpdateTime }) > 0;
    }

    public async Task<PageQueryResult<AppCodeEntity>> PageAsync(AppCodePageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<AppCodeEntity>.Build(OrderByType.Desc, entity => entity.Id);

        return await appCodeRepository.PageQueryAsync(
            WhereExpressionUtil.Create<AppCodeEntity>(entity => !entity.IsDelete)
                .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Code), entity => entity.Code.Contains(request.Code))
                .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Name), entity => entity.Name.Contains(request.Name)),
            orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<AppCodeEntity> GetByIdAsync(long id)
    {
        return await appCodeRepository.GetByIdAsync(id);
    }

    public async Task<List<AppCodeDto>> GetActiveListAsync()
    {
        var entities = await appCodeRepository.QueryAsync(
            entity => entity.State == CommonState.Enable && !entity.IsDelete);
        return entities?.Select(mapper.Map<AppCodeDto>).ToList() ?? new List<AppCodeDto>();
    }
}