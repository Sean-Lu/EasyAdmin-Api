using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.SeedData;

/// <summary>
/// 代码生成分类种子数据
/// </summary>
public class CodeGenCategorySeedData : IEntitySeedData<CodeGenCategoryEntity>
{
    public IEnumerable<CodeGenCategoryEntity> SeedData()
    {
        var now = DateTime.Now;
        
        return new[]
        {
            new CodeGenCategoryEntity
            {
                Id = 1,
                Name = "C#项目模板(示例)",
                Code = "csharp",
                SortOrder = 2,
                Description = "C#项目代码模板分类",
                IsBuiltIn = true,
                State = CommonState.Enable,
                CreateTime = now,
                UpdateTime = now
            },
            new CodeGenCategoryEntity
            {
                Id = 2,
                Name = "Java项目模板(示例)",
                Code = "java",
                SortOrder = 1,
                Description = "Java项目代码模板分类",
                IsBuiltIn = true,
                State = CommonState.Enable,
                CreateTime = now,
                UpdateTime = now
            }
        };
    }
}