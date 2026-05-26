using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Wrapper;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

public class CodeGenCategoryService(
    ILogger<CodeGenCategoryService> logger,
    ICodeGenCategoryRepository categoryRepository,
    IMapper mapper)
    : ICodeGenCategoryService
{
    public async Task<List<CodeGenCategoryDto>> GetTreeAsync()
    {
        var orderBy = OrderByConditionBuilder<CodeGenCategoryEntity>.Build(OrderByType.Desc, entity => entity.SortOrder);
        orderBy.Next = OrderByConditionBuilder<CodeGenCategoryEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        orderBy.Next.Next = OrderByConditionBuilder<CodeGenCategoryEntity>.Build(OrderByType.Desc, entity => entity.Id);
        var categories = await categoryRepository.QueryAsync(
            WhereExpressionUtil.Create<CodeGenCategoryEntity>(e => e.State == CommonState.Enable && !e.IsDelete),
            orderBy
        );

        return mapper.Map<List<CodeGenCategoryDto>>(categories?.ToList() ?? new List<CodeGenCategoryEntity>());
    }

    public async Task<List<CodeGenCategoryDto>> GetListAsync()
    {
        var orderBy = OrderByConditionBuilder<CodeGenCategoryEntity>.Build(OrderByType.Desc, entity => entity.SortOrder);
        orderBy.Next = OrderByConditionBuilder<CodeGenCategoryEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        orderBy.Next.Next = OrderByConditionBuilder<CodeGenCategoryEntity>.Build(OrderByType.Desc, entity => entity.Id);
        var categories = await categoryRepository.QueryAsync(
            WhereExpressionUtil.Create<CodeGenCategoryEntity>(e => !e.IsDelete),
            orderBy
        );

        return mapper.Map<List<CodeGenCategoryDto>>(categories?.ToList() ?? new List<CodeGenCategoryEntity>());
    }

    public async Task<CodeGenCategoryDto> GetByIdAsync(long id)
    {
        var category = await categoryRepository.GetByIdAsync(id);
        if (category == null || category.IsDelete)
        {
            throw new ExplicitException("分类不存在");
        }
        return mapper.Map<CodeGenCategoryDto>(category);
    }

    public async Task<long> AddAsync(CodeGenCategoryAddDto dto)
    {
        await ValidateAddDtoAsync(dto);

        var category = mapper.Map<CodeGenCategoryEntity>(dto);
        category.IsBuiltIn = false;
        category.State = CommonState.Enable;
        await categoryRepository.AddAsync(category);
        return category.Id;
    }

    public async Task<bool> UpdateAsync(CodeGenCategoryUpdateDto dto)
    {
        return await categoryRepository.UpdateByDtoAsync(dto, mapper.Map<CodeGenCategoryEntity>) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var category = await categoryRepository.GetByIdAsync(id);
        if (category == null || category.IsDelete)
        {
            throw new ExplicitException("分类不存在");
        }
        if (category.IsBuiltIn)
        {
            throw new ExplicitException("内置分类不允许删除");
        }

        return await categoryRepository.DeleteByIdAsync(id);
    }

    public async Task<string> ExportAsync()
    {
        var categories = await GetListAsync();
        return JsonConvert.SerializeObject(categories, Formatting.Indented);
    }

    public async Task ImportAsync(CodeGenCategoryImportDto dto)
    {
        if (dto.Categories == null || !dto.Categories.Any())
        {
            throw new ExplicitException("没有需要导入的分类数据");
        }

        foreach (var item in dto.Categories)
        {
            await ValidateAddDtoAsync(item);
            await AddAsync(item);
        }
    }

    private async Task ValidateAddDtoAsync(CodeGenCategoryAddDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ExplicitException("分类名称不能为空");
        }
        if (string.IsNullOrWhiteSpace(dto.Code))
        {
            throw new ExplicitException("分类编码不能为空");
        }
        if (await categoryRepository.ExistsAsync(WhereExpressionUtil.Create<CodeGenCategoryEntity>(e => e.Code == dto.Code && !e.IsDelete)))
        {
            throw new ExplicitException("分类编码已存在");
        }
    }
}
