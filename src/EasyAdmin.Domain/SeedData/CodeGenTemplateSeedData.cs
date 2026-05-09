using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.SeedData;

/// <summary>
/// 代码生成内置模板种子数据
/// </summary>
public class CodeGenTemplateSeedData : IEntitySeedData<CodeGenTemplateEntity>
{
    public IEnumerable<CodeGenTemplateEntity> SeedData()
    {
        return new[]
        {
            new CodeGenTemplateEntity
            {
                Id = 1,
                Name = "Entity实体(MyBatis)",
                Code = "entity_mybatis",
                CategoryId = 1,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "entity/{{ClassName}}.java",
                IsDefault = true,
                State = CommonState.Enable,
                Description = "MyBatis实体模板",
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
            new CodeGenTemplateEntity
            {
                Id = 2,
                Name = "Entity实体(EF Core)",
                Code = "entity_efcore",
                CategoryId = 2,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "Entities/{{ClassName}}.cs",
                IsDefault = false,
                State = CommonState.Enable,
                Description = "EF Core实体模板",
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
            new CodeGenTemplateEntity
            {
                Id = 3,
                Name = "Mapper接口(MyBatis)",
                Code = "mapper_mybatis",
                CategoryId = 1,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "mapper/{{ClassName}}Mapper.java",
                IsDefault = true,
                State = CommonState.Enable,
                Description = "MyBatis Mapper接口",
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
            new CodeGenTemplateEntity
            {
                Id = 4,
                Name = "Service接口",
                Code = "service_interface",
                CategoryId = 1,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "service/I{{ClassName}}Service.java",
                IsDefault = true,
                State = CommonState.Enable,
                Description = "Service接口模板",
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
            new CodeGenTemplateEntity
            {
                Id = 5,
                Name = "Service实现",
                Code = "service_impl",
                CategoryId = 1,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "service/impl/{{ClassName}}ServiceImpl.java",
                IsDefault = true,
                State = CommonState.Enable,
                Description = "Service实现模板",
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
            new CodeGenTemplateEntity
            {
                Id = 6,
                Name = "Controller",
                Code = "controller",
                CategoryId = 1,
                TemplateType = CodeGenTemplateType.BuiltIn,
                FilePath = "controller/{{ClassName}}Controller.java",
                IsDefault = true,
                State = CommonState.Enable,
                Description = "Controller模板",
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
            }
        };
    }
}