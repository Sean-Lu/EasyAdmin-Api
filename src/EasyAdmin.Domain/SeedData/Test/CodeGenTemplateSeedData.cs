using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.SeedData.Test;

/// <summary>
/// 测试 - 代码生成内置模板种子数据
/// </summary>
public class CodeGenTemplateSeedData : IEntitySeedData<CodeGenTemplateEntity>, ITestSeedData
{
    public IEnumerable<CodeGenTemplateEntity> SeedData()
    {
        return new[]
        {
            #region 内置C#项目模板（示例）
            // Entity实体
            new CodeGenTemplateEntity
            {
                Id = 1,
                Name = "Entity实体(C#)",
                Code = "csharp_entity",
                CategoryId = 1,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "Entities/{{ClassName}}Entity.cs",
                IsDefault = true,
                SortOrder = 1,
                State = CommonState.Enable,
                Description = "C# Entity实体模板",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Content = @"using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace {{PackageName}}.{{ModuleName}}.Entities
{
    /// <summary>
    /// {{TableComment}}
    /// </summary>
    [Table(""{{TableName}}"")]
    public class {{ClassName}}Entity
    {
{{#each Columns}}
        /// <summary>
        /// {{ColumnComment}}
        /// </summary>
{{#if IsKey}}
        [Key]
{{/if}}
{{#if IsIdentity}}
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
{{/if}}
        public {{CSharpType}} {{PropertyName}} { get; set; }
{{/each}}
    }
}"
            },
            // Repository接口
            new CodeGenTemplateEntity
            {
                Id = 2,
                Name = "Repository接口(C#)",
                Code = "csharp_repository_interface",
                CategoryId = 1,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "Contracts/I{{ClassName}}Repository.cs",
                IsDefault = true,
                SortOrder = 2,
                State = CommonState.Enable,
                Description = "C# Repository接口模板",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Content = @"using EasyAdmin.Domain.Entities;

namespace {{PackageName}}.{{ModuleName}}.Contracts;

public interface I{{ClassName}}Repository : IBaseRepositoryExt<{{ClassName}}Entity>
{
}"
            },
            // Repository实现
            new CodeGenTemplateEntity
            {
                Id = 3,
                Name = "Repository实现(C#)",
                Code = "csharp_repository_impl",
                CategoryId = 1,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "Repositories/{{ClassName}}Repository.cs",
                IsDefault = true,
                SortOrder = 3,
                State = CommonState.Enable,
                Description = "C# Repository实现模板",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Content = @"using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace {{PackageName}}.{{ModuleName}}.Repositories;

public class {{ClassName}}Repository(
    IConfiguration configuration,
    ILogger<{{ClassName}}Repository> logger
    ) : BaseRepositoryExt<{{ClassName}}Entity>(configuration, logger), I{{ClassName}}Repository
{
}"
            },
            // Service接口
            new CodeGenTemplateEntity
            {
                Id = 4,
                Name = "Service接口(C#)",
                Code = "csharp_service_interface",
                CategoryId = 1,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "Contracts/I{{ClassName}}Service.cs",
                IsDefault = true,
                SortOrder = 4,
                State = CommonState.Enable,
                Description = "C# Service接口模板",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Content = @"using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace {{PackageName}}.{{ModuleName}}.Contracts;

public interface I{{ClassName}}Service
{
    Task<bool> AddAsync({{ClassName}}AddDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> UpdateAsync({{ClassName}}UpdateDto dto);
    Task<bool> UpdateStateAsync(long id, CommonState state);
    Task<PageQueryResult<{{ClassName}}Entity>> PageAsync({{ClassName}}PageReqDto request);
    Task<{{ClassName}}Entity> GetByIdAsync(long id);
}"
            },
            // Service实现
            new CodeGenTemplateEntity
            {
                Id = 5,
                Name = "Service实现(C#)",
                Code = "csharp_service_impl",
                CategoryId = 1,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "Services/{{ClassName}}Service.cs",
                IsDefault = true,
                SortOrder = 5,
                State = CommonState.Enable,
                Description = "C# Service实现模板",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Content = @"using {{PackageName}}.{{ModuleName}}.Contracts;
using {{PackageName}}.{{ModuleName}}.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using MapsterMapper;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace {{PackageName}}.{{ModuleName}}.Services;

public class {{ClassName}}Service(
    ILogger<{{ClassName}}Service> logger,
    IMapper mapper,
    I{{ClassName}}Repository {{InstanceName}}Repository
) : I{{ClassName}}Service
{
    public async Task<bool> AddAsync({{ClassName}}Dto dto)
    {
        var entity = mapper.Map<{{ClassName}}Entity>(dto);
        return await {{InstanceName}}Repository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await {{InstanceName}}Repository.DeleteByIdAsync(id);
    }

    public async Task<bool> UpdateAsync({{ClassName}}UpdateDto dto)
    {
        return await {{InstanceName}}Repository.UpdateByDtoAsync(dto, mapper.Map<{{ClassName}}Entity>) > 0;
    }

    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await {{InstanceName}}Repository.UpdateAsync(new {{ClassName}}Entity { State = state }, entity => new { entity.State }, entity => entity.Id == id) > 0;
    }

    public async Task<PageQueryResult<{{ClassName}}Entity>> PageAsync({{ClassName}}PageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<{{ClassName}}Entity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        orderBy.Next = OrderByConditionBuilder<{{ClassName}}Entity>.Build(OrderByType.Desc, entity => entity.Id);
        return await {{InstanceName}}Repository.PageQueryAsync(
            WhereExpressionUtil.Create<{{ClassName}}Entity>(entity => !entity.IsDelete),
            orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<{{ClassName}}Entity> GetByIdAsync(long id)
    {
        return await {{InstanceName}}Repository.GetByIdAsync(id);
    }
}"
            },
            // Controller
            new CodeGenTemplateEntity
            {
                Id = 6,
                Name = "Controller(C#)",
                Code = "csharp_controller",
                CategoryId = 1,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "Controllers/{{ClassName}}Controller.cs",
                IsDefault = true,
                SortOrder = 6,
                State = CommonState.Enable,
                Description = "C# Controller模板",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Content = @"using {{PackageName}}.{{ModuleName}}.Contracts;
using {{PackageName}}.{{ModuleName}}.Dtos;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Web.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace {{PackageName}}.{{ModuleName}}.Controllers;

public class {{ClassName}}Controller(
    ILogger<{{ClassName}}Controller> logger,
    IMapper mapper,
    I{{ClassName}}Service {{InstanceName}}Service
) : BaseApiController
{
    /// <summary>
    /// 新增
    /// </summary>
    /// <param name=""data""></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Add({{ClassName}}AddDto data)
    {
        return Success(await {{InstanceName}}Service.AddAsync(data));
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name=""data""></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var ids = data[""ids""]?.Values<long>().ToList() ?? default;
        if (ids != null && ids.Any())
        {
            // 批量删除
            return Success(await {{InstanceName}}Service.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data[""id""]?.Value<long>() ?? default;
        return Success(await {{InstanceName}}Service.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name=""data""></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Update({{ClassName}}UpdateDto data)
    {
        return Success(await {{InstanceName}}Service.UpdateAsync(data));
    }

    /// <summary>
    /// 修改状态
    /// </summary>
    /// <param name=""data""></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data[""id""]?.Value<long>() ?? default;
        var state = (CommonState)(data[""state""]?.Value<int>() ?? default);
        return Success(await {{InstanceName}}Service.UpdateStateAsync(id, state));
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name=""request""></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<{{ClassName}}Dto>>> Page([FromQuery] {{ClassName}}PageReqDto request)
    {
        var pageResult = await {{InstanceName}}Service.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<{{ClassName}}Dto>>(pageResult));
    }

    /// <summary>
    /// 查询详情
    /// </summary>
    /// <param name=""id""></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<{{ClassName}}Dto>> Detail(long id)
    {
        var entity = await {{InstanceName}}Service.GetByIdAsync(id);
        return Success(mapper.Map<{{ClassName}}Dto>(entity));
    }
}"
            },
            #endregion

            #region 内置Java项目模板（示例）
            // Entity实体
            new CodeGenTemplateEntity
            {
                Id = 10,
                Name = "Entity实体(MyBatis)",
                Code = "java_entity_mybatis",
                CategoryId = 2,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "entity/{{ClassName}}.java",
                IsDefault = true,
                SortOrder = 1,
                State = CommonState.Enable,
                Description = "Java MyBatis实体模板",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Content = @"package {{PackageName}}.{{ModuleName}}.entity;

import lombok.Data;
import java.io.Serializable;
{{#each Columns}}
{{#if (or (IsKey) (IsIdentity))}}
import java.util.Date;
{{/if}}
{{/each}}

/**
 * {{TableComment}}
 * 
 * @author {{Author}}
 * @since {{Date}}
 */
@Data
public class {{ClassName}} {

{{#each Columns}}
    /**
     * {{ColumnComment}}
     */
    private {{JavaType}} {{FieldName}};

{{/each}}
}"
            },
            // Mapper接口
            new CodeGenTemplateEntity
            {
                Id = 11,
                Name = "Mapper接口(MyBatis)",
                Code = "java_mapper_mybatis",
                CategoryId = 2,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "mapper/{{ClassName}}Mapper.java",
                IsDefault = true,
                SortOrder = 2,
                State = CommonState.Enable,
                Description = "Java MyBatis Mapper接口",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Content = @"package {{PackageName}}.{{ModuleName}}.mapper;

import {{PackageName}}.{{ModuleName}}.entity.{{ClassName}};
import org.apache.ibatis.annotations.Mapper;

/**
 * {{TableComment}} Mapper
 * 
 * @author {{Author}}
 * @since {{Date}}
 */
@Mapper
public interface {{ClassName}}Mapper {

    int insert({{ClassName}} entity);

    int updateById({{ClassName}} entity);

    int deleteById({{#each Columns}}{{#if IsKey}}{{FieldName}}{{/if}}{{/each}});

    {{ClassName}} selectById({{#each Columns}}{{#if IsKey}}{{JavaType}} {{FieldName}}{{/if}}{{/each}});
}"
            },
            // Repository接口
            new CodeGenTemplateEntity
            {
                Id = 12,
                Name = "Repository接口(Java)",
                Code = "java_repository_interface",
                CategoryId = 2,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "repository/{{ClassName}}Repository.java",
                IsDefault = true,
                SortOrder = 3,
                State = CommonState.Enable,
                Description = "Java Repository接口模板",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Content = @"package {{PackageName}}.{{ModuleName}}.repository;

import {{PackageName}}.{{ModuleName}}.entity.{{ClassName}};
import java.util.List;

/**
 * {{TableComment}} Repository
 * 
 * @author {{Author}}
 * @since {{Date}}
 */
public interface {{ClassName}}Repository {

    int insert({{ClassName}} entity);

    int updateById({{ClassName}} entity);

    int deleteById({{#each Columns}}{{#if IsKey}}{{JavaType}} {{FieldName}}{{/if}}{{/each}});

    {{ClassName}} selectById({{#each Columns}}{{#if IsKey}}{{JavaType}} {{FieldName}}{{/if}}{{/each}});

    List<{{ClassName}}> selectAll();
}"
            },
            // Repository实现
            new CodeGenTemplateEntity
            {
                Id = 13,
                Name = "Repository实现(Java)",
                Code = "java_repository_impl",
                CategoryId = 2,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "repository/impl/{{ClassName}}RepositoryImpl.java",
                IsDefault = true,
                SortOrder = 4,
                State = CommonState.Enable,
                Description = "Java Repository实现模板",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Content = @"package {{PackageName}}.{{ModuleName}}.repository.impl;

import {{PackageName}}.{{ModuleName}}.entity.{{ClassName}};
import {{PackageName}}.{{ModuleName}}.repository.{{ClassName}}Repository;
import org.springframework.stereotype.Repository;
import org.springframework.beans.factory.annotation.Autowired;
import javax.sql.DataSource;
import java.sql.*;
import java.util.List;
import java.util.ArrayList;

/**
 * {{TableComment}} Repository实现
 * 
 * @author {{Author}}
 * @since {{Date}}
 */
@Repository
public class {{ClassName}}RepositoryImpl implements {{ClassName}}Repository {

    @Autowired
    private DataSource dataSource;

    @Override
    public int insert({{ClassName}} entity) {
        // TODO: 实现插入逻辑
        return 1;
    }

    @Override
    public int updateById({{ClassName}} entity) {
        // TODO: 实现更新逻辑
        return 1;
    }

    @Override
    public int deleteById({{#each Columns}}{{#if IsKey}}{{JavaType}} {{FieldName}}{{/if}}{{/each}}) {
        // TODO: 实现删除逻辑
        return 1;
    }

    @Override
    public {{ClassName}} selectById({{#each Columns}}{{#if IsKey}}{{JavaType}} {{FieldName}}{{/if}}{{/each}}) {
        // TODO: 实现查询逻辑
        return null;
    }

    @Override
    public List<{{ClassName}}> selectAll() {
        // TODO: 实现查询逻辑
        return new ArrayList<>();
    }
}"
            },
            // Service接口
            new CodeGenTemplateEntity
            {
                Id = 14,
                Name = "Service接口(Java)",
                Code = "java_service_interface",
                CategoryId = 2,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "service/I{{ClassName}}Service.java",
                IsDefault = true,
                SortOrder = 5,
                State = CommonState.Enable,
                Description = "Java Service接口模板",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Content = @"package {{PackageName}}.{{ModuleName}}.service;

import {{PackageName}}.{{ModuleName}}.entity.{{ClassName}};

/**
 * {{TableComment}} Service
 * 
 * @author {{Author}}
 * @since {{Date}}
 */
public interface I{{ClassName}}Service {

    int add({{ClassName}} entity);

    int update({{ClassName}} entity);

    int deleteById({{#each Columns}}{{#if IsKey}}{{JavaType}} {{FieldName}}{{/if}}{{/each}});

    {{ClassName}} getById({{#each Columns}}{{#if IsKey}}{{JavaType}} {{FieldName}}{{/if}}{{/each}});
}"
            },
            // Service实现
            new CodeGenTemplateEntity
            {
                Id = 15,
                Name = "Service实现(Java)",
                Code = "java_service_impl",
                CategoryId = 2,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "service/impl/{{ClassName}}ServiceImpl.java",
                IsDefault = true,
                SortOrder = 6,
                State = CommonState.Enable,
                Description = "Java Service实现模板",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Content = @"package {{PackageName}}.{{ModuleName}}.service.impl;

import {{PackageName}}.{{ModuleName}}.entity.{{ClassName}};
import {{PackageName}}.{{ModuleName}}.mapper.{{ClassName}}Mapper;
import {{PackageName}}.{{ModuleName}}.service.I{{ClassName}}Service;
import org.springframework.stereotype.Service;
import org.springframework.beans.factory.annotation.Autowired;

/**
 * {{TableComment}} Service实现
 * 
 * @author {{Author}}
 * @since {{Date}}
 */
@Service
public class {{ClassName}}ServiceImpl implements I{{ClassName}}Service {

    @Autowired
    private {{ClassName}}Mapper {{InstanceName}}Mapper;

    @Override
    public int add({{ClassName}} entity) {
        return {{InstanceName}}Mapper.insert(entity);
    }

    @Override
    public int update({{ClassName}} entity) {
        return {{InstanceName}}Mapper.updateById(entity);
    }

    @Override
    public int deleteById({{#each Columns}}{{#if IsKey}}{{JavaType}} {{FieldName}}{{/if}}{{/each}}) {
        return {{InstanceName}}Mapper.deleteById({{#each Columns}}{{#if IsKey}}{{FieldName}}{{/if}}{{/each}});
    }

    @Override
    public {{ClassName}} getById({{#each Columns}}{{#if IsKey}}{{JavaType}} {{FieldName}}{{/if}}{{/each}}) {
        return {{InstanceName}}Mapper.selectById({{#each Columns}}{{#if IsKey}}{{FieldName}}{{/if}}{{/each}});
    }
}"
            },
            // Controller
            new CodeGenTemplateEntity
            {
                Id = 16,
                Name = "Controller(Java)",
                Code = "java_controller",
                CategoryId = 2,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "controller/{{ClassName}}Controller.java",
                IsDefault = true,
                SortOrder = 7,
                State = CommonState.Enable,
                Description = "Java Controller模板",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Content = @"package {{PackageName}}.{{ModuleName}}.controller;

import {{PackageName}}.{{ModuleName}}.entity.{{ClassName}};
import {{PackageName}}.{{ModuleName}}.service.I{{ClassName}}Service;
import org.springframework.web.bind.annotation.*;
import org.springframework.beans.factory.annotation.Autowired;

/**
 * {{TableComment}} Controller
 * 
 * @author {{Author}}
 * @since {{Date}}
 */
@RestController
@RequestMapping(""/{{InstanceName}}"")
public class {{ClassName}}Controller {

    @Autowired
    private I{{ClassName}}Service {{InstanceName}}Service;

    @PostMapping(""/add"")
    public int add(@RequestBody {{ClassName}} entity) {
        return {{InstanceName}}Service.add(entity);
    }

    @PostMapping(""/update"")
    public int update(@RequestBody {{ClassName}} entity) {
        return {{InstanceName}}Service.update(entity);
    }

    @PostMapping(""/delete"")
    public int deleteById(@RequestParam {{#each Columns}}{{#if IsKey}}{{JavaType}} {{FieldName}}{{/if}}{{/each}}) {
        return {{InstanceName}}Service.deleteById({{#each Columns}}{{#if IsKey}}{{FieldName}}{{/if}}{{/each}});
    }

    @GetMapping(""/get"")
    public {{ClassName}} getById(@RequestParam {{#each Columns}}{{#if IsKey}}{{JavaType}} {{FieldName}}{{/if}}{{/each}}) {
        return {{InstanceName}}Service.getById({{#each Columns}}{{#if IsKey}}{{FieldName}}{{/if}}{{/each}});
    }
}"
            },
            #endregion
        };
    }
}