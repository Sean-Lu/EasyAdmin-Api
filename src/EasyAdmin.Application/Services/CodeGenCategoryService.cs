using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
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
        var orderBy = OrderByConditionBuilder<CodeGenCategoryEntity>.Build(OrderByType.Asc, entity => entity.SortOrder);
        orderBy.Next = OrderByConditionBuilder<CodeGenCategoryEntity>.Build(OrderByType.Asc, entity => entity.Id);
        var categories = await categoryRepository.QueryAsync(
            WhereExpressionUtil.Create<CodeGenCategoryEntity>(e => e.State == CommonState.Enable && !e.IsDelete),
            orderBy
        );

        return mapper.Map<List<CodeGenCategoryDto>>(categories?.ToList() ?? new List<CodeGenCategoryEntity>());
    }

    public async Task<List<CodeGenCategoryDto>> GetListAsync()
    {
        var orderBy = OrderByConditionBuilder<CodeGenCategoryEntity>.Build(OrderByType.Asc, entity => entity.SortOrder);
        orderBy.Next = OrderByConditionBuilder<CodeGenCategoryEntity>.Build(OrderByType.Asc, entity => entity.Id);
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
            throw new Exception("分类不存在");
        }
        return mapper.Map<CodeGenCategoryDto>(category);
    }

    public async Task<long> AddAsync(CodeGenCategoryAddDto request)
    {
        ValidateAddRequest(request);

        var category = new CodeGenCategoryEntity
        {
            Name = request.Name,
            Code = request.Code,
            SortOrder = request.SortOrder,
            Description = request.Description,
            IsBuiltIn = false,
            State = CommonState.Enable,
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now
        };

        await categoryRepository.AddAsync(category);

        logger.LogInformation("新增分类: {Name}", request.Name);
        return category.Id;
    }

    public async Task UpdateAsync(CodeGenCategoryUpdateDto request)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id);
        if (category == null || category.IsDelete)
        {
            throw new Exception("分类不存在");
        }

        if (category.IsBuiltIn)
        {
            throw new Exception("内置分类不允许修改");
        }

        category.Name = request.Name;
        category.Code = request.Code;
        category.SortOrder = request.SortOrder;
        category.Description = request.Description;
        category.State = (CommonState)request.State;
        category.UpdateTime = DateTime.Now;

        await categoryRepository.UpdateAsync(category);

        logger.LogInformation("更新分类: {Id}", request.Id);
    }

    public async Task DeleteAsync(long id)
    {
        var category = await categoryRepository.GetByIdAsync(id);
        if (category == null || category.IsDelete)
        {
            throw new Exception("分类不存在");
        }

        if (category.IsBuiltIn)
        {
            throw new Exception("内置分类不允许删除");
        }

        category.IsDelete = true;
        category.UpdateTime = DateTime.Now;
        await categoryRepository.UpdateAsync(category);

        logger.LogInformation("删除分类: {Id}", id);
    }

    public async Task<string> ExportAsync()
    {
        var categories = await GetListAsync();
        return JsonConvert.SerializeObject(categories, Formatting.Indented);
    }

    public async Task ImportAsync(CodeGenCategoryImportDto request)
    {
        if (request.Categories == null || !request.Categories.Any())
        {
            throw new Exception("没有需要导入的分类数据");
        }

        foreach (var item in request.Categories)
        {
            ValidateAddRequest(item);
        }

        foreach (var item in request.Categories)
        {
            try
            {
                var orderBy = OrderByConditionBuilder<CodeGenCategoryEntity>.Build(OrderByType.Asc, entity => entity.Id);
                var existingList = await categoryRepository.QueryAsync(
                    WhereExpressionUtil.Create<CodeGenCategoryEntity>(e => e.Code == item.Code && !e.IsDelete),
                    orderBy
                );
                var existing = existingList?.FirstOrDefault();

                if (existing == null)
                {
                    await AddAsync(item);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning("导入分类失败: {Name}, 错误: {Message}", item.Name, ex.Message);
            }
        }

        logger.LogInformation("导入分类完成，共 {Count} 条", request.Categories.Count);
    }

    private void ValidateAddRequest(CodeGenCategoryAddDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new Exception("分类名称不能为空");
        }
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            throw new Exception("分类编码不能为空");
        }

        var orderBy = OrderByConditionBuilder<CodeGenCategoryEntity>.Build(OrderByType.Asc, entity => entity.Id);
        var existingList = categoryRepository.Query(
            WhereExpressionUtil.Create<CodeGenCategoryEntity>(e => e.Code == request.Code && !e.IsDelete),
            orderBy
        );
        var existing = existingList?.FirstOrDefault();
        if (existing != null)
        {
            throw new Exception("分类编码已存在");
        }
    }
}
