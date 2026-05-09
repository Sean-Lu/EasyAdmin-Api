using System.Data;
using System.Data.Common;
using System.IO.Compression;
using System.Text;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using HandlebarsDotNet;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Npgsql;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 代码生成服务
/// </summary>
public class CodeGenService(
    ILogger<CodeGenService> logger,
    IMapper mapper,
    ICodeGenTemplateRepository templateRepository,
    IDbConnectionConfigRepository dbConfigRepository
    ) : ICodeGenService
{
    #region 模板管理

    public async Task<List<CodeGenTemplateDto>> GetTemplateListAsync(CodeGenTemplateListReqDto request)
    {
        var orderBy = OrderByConditionBuilder<CodeGenTemplateEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        var list = await templateRepository.QueryAsync(
            WhereExpressionUtil.Create<CodeGenTemplateEntity>(entity => !entity.IsDelete)
                .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Name), entity => entity.Name.Contains(request.Name))
                .AndAlsoIF(request.TemplateType != null, entity => entity.TemplateType == request.TemplateType)
                .AndAlsoIF(request.State != null, entity => entity.State == request.State)
                .AndAlsoIF(request.CategoryId != null && request.CategoryId > 0, entity => entity.CategoryId == request.CategoryId),
            orderBy: orderBy
        );

        return mapper.Map<List<CodeGenTemplateDto>>(list?.ToList() ?? new List<CodeGenTemplateEntity>());
    }

    public async Task<CodeGenTemplateDto> GetTemplateByIdAsync(long id)
    {
        var entity = await templateRepository.GetByIdAsync(id);
        return mapper.Map<CodeGenTemplateDto>(entity);
    }

    public async Task<bool> AddTemplateAsync(CodeGenTemplateDto dto)
    {
        var entity = mapper.Map<CodeGenTemplateEntity>(dto);
        entity.CreateTime = DateTime.Now;
        entity.UpdateTime = DateTime.Now;

        if (entity.IsDefault)
        {
            await SetOtherTemplatesNotDefaultAsync();
        }

        return await templateRepository.AddAsync(entity);
    }

    public async Task<bool> UpdateTemplateAsync(CodeGenTemplateUpdateDto dto)
    {
        var entity = await templateRepository.GetByIdAsync(dto.Id);
        if (entity == null || entity.Id <= 0)
        {
            throw new Exception("模板不存在");
        }

        if (dto.IsDefault && !entity.IsDefault)
        {
            await SetOtherTemplatesNotDefaultAsync();
        }

        entity.Name = dto.Name;
        entity.Code = dto.Code;
        entity.Description = dto.Description;
        entity.FilePath = dto.FilePath;
        entity.IsDefault = dto.IsDefault;
        entity.State = dto.State;
        entity.UpdateTime = DateTime.Now;

        return await templateRepository.UpdateAsync(entity) > 0;
    }

    public async Task<bool> DeleteTemplateAsync(long id)
    {
        return await templateRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> DeleteTemplatesAsync(List<long> ids)
    {
        return await templateRepository.DeleteByIdsAsync(ids);
    }

    private async Task SetOtherTemplatesNotDefaultAsync()
    {
        await templateRepository.UpdateAsync(
            new CodeGenTemplateEntity { IsDefault = false },
            entity => new { entity.IsDefault },
            entity => entity.IsDefault && !entity.IsDelete
        );
    }

    #endregion

    #region 数据库配置管理

    public async Task<List<DbConnectionConfigDto>> GetDbConfigListAsync(DbConnectionConfigListReqDto request)
    {
        var orderBy = OrderByConditionBuilder<DbConnectionConfigEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        var list = await dbConfigRepository.QueryAsync(
            WhereExpressionUtil.Create<DbConnectionConfigEntity>(entity => !entity.IsDelete)
                .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Name), entity => entity.Name.Contains(request.Name))
                .AndAlsoIF(request.DbType != null, entity => entity.DbType == request.DbType)
                .AndAlsoIF(request.State != null, entity => entity.State == request.State),
            orderBy: orderBy
        );

        var entities = list?.ToList() ?? new List<DbConnectionConfigEntity>();
        var result = mapper.Map<List<DbConnectionConfigDto>>(entities);
        for (var i = 0; i < result.Count; i++)
        {
            result[i].Password = DecryptPassword(entities[i].Password);
        }
        return result;
    }

    public async Task<DbConnectionConfigDto> GetDbConfigByIdAsync(long id)
    {
        var entity = await dbConfigRepository.GetByIdAsync(id);
        return mapper.Map<DbConnectionConfigDto>(entity);
    }

    public async Task<bool> AddDbConfigAsync(DbConnectionConfigUpdateDto dto)
    {
        var entity = mapper.Map<DbConnectionConfigEntity>(dto);
        entity.Password = EncryptPassword(dto.Password);
        entity.CreateTime = DateTime.Now;
        entity.UpdateTime = DateTime.Now;

        if (entity.IsDefault)
        {
            await SetOtherDbConfigsNotDefaultAsync();
        }

        return await dbConfigRepository.AddAsync(entity);
    }

    public async Task<bool> UpdateDbConfigAsync(DbConnectionConfigUpdateDto dto)
    {
        var entity = await dbConfigRepository.GetByIdAsync(dto.Id);
        if (entity == null || entity.Id <= 0)
        {
            throw new Exception("数据库配置不存在");
        }

        if (dto.IsDefault && !entity.IsDefault)
        {
            await SetOtherDbConfigsNotDefaultAsync();
        }

        entity.Name = dto.Name;
        entity.DbType = dto.DbType;
        entity.Host = dto.Host;
        entity.Port = dto.Port;
        entity.Database = dto.Database;
        entity.Username = dto.Username;
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            entity.Password = EncryptPassword(dto.Password);
        }
        entity.ConnectionString = dto.ConnectionString;
        entity.IsDefault = dto.IsDefault;
        entity.State = dto.State;
        entity.UpdateTime = DateTime.Now;

        return await dbConfigRepository.UpdateAsync(entity) > 0;
    }

    public async Task<bool> DeleteDbConfigAsync(long id)
    {
        return await dbConfigRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> DeleteDbConfigsAsync(List<long> ids)
    {
        return await dbConfigRepository.DeleteByIdsAsync(ids);
    }

    private async Task SetOtherDbConfigsNotDefaultAsync()
    {
        await dbConfigRepository.UpdateAsync(
            new DbConnectionConfigEntity { IsDefault = false },
            entity => new { entity.IsDefault },
            entity => entity.IsDefault && !entity.IsDelete
        );
    }

    public async Task<bool> TestDbConnectionAsync(long id)
    {
        var entity = await dbConfigRepository.GetByIdAsync(id);
        if (entity == null || entity.Id <= 0)
        {
            throw new Exception("数据库配置不存在");
        }

        try
        {
            using var connection = CreateDbConnection(entity);
            await connection.OpenAsync();
            await connection.CloseAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "测试数据库连接失败");
            throw new Exception($"数据库连接失败: {ex.Message}");
        }
    }

    public async Task<List<DbTableInfoDto>> GetDbTablesAsync(long id)
    {
        var entity = await dbConfigRepository.GetByIdAsync(id);
        if (entity == null || entity.Id <= 0)
        {
            throw new Exception("数据库配置不存在");
        }

        try
        {
            using var connection = CreateDbConnection(entity);
            await connection.OpenAsync();

            var tables = await GetTablesAsync(connection, entity.DbType);
            var result = new List<DbTableInfoDto>();

            foreach (var tableName in tables)
            {
                var columns = await GetColumnsAsync(connection, entity.DbType, tableName);
                var tableComment = await GetTableCommentAsync(connection, entity.DbType, tableName);
                result.Add(new DbTableInfoDto
                {
                    TableName = tableName,
                    TableComment = tableComment,
                    Columns = columns
                });
            }

            await connection.CloseAsync();
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取数据库表列表失败");
            throw new Exception($"获取表列表失败: {ex.Message}");
        }
    }

    #endregion

    #region 代码生成

    private static readonly Dictionary<string, CodeGenResultDto> GeneratedCodeCache = new();

    public async Task<CodeGenResultDto> GenerateCodeAsync(CodeGenReqDto request)
    {
        var dbConfig = await dbConfigRepository.GetByIdAsync(request.DbConfigId);
        if (dbConfig == null || dbConfig.Id <= 0)
        {
            throw new Exception("数据库配置不存在");
        }

        var templates = (await templateRepository.QueryAsync(
            WhereExpressionUtil.Create<CodeGenTemplateEntity>(entity => !entity.IsDelete && entity.State == CommonState.Enable)
                .AndAlsoIF(request.TemplateIds != null && request.TemplateIds.Any(), entity => request.TemplateIds.Contains(entity.Id))
        ))?.ToList();

        if (templates == null || !templates.Any())
        {
            throw new Exception("没有可用的代码模板");
        }

        using var connection = CreateDbConnection(dbConfig);
        await connection.OpenAsync();

        var resultFiles = new List<CodeGenFileDto>();
        var taskId = Guid.NewGuid().ToString("N");

        foreach (var tableName in request.TableNames)
        {
            var tableInfo = await GetTableInfoAsync(connection, dbConfig.DbType, tableName);
            var className = ConvertToPascalCase(RemovePrefix(tableName, request.TablePrefix));

            var context = new
            {
                TableName = tableInfo.TableName,
                ClassName = className,
                InstanceName = ConvertToCamelCase(className),
                TableComment = tableInfo.TableComment,
                PackageName = request.PackageName,
                ModuleName = request.ModuleName,
                Author = request.Author,
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                Columns = tableInfo.Columns.Select(c => new
                {
                    ColumnName = c.ColumnName,
                    FieldName = ConvertToCamelCase(c.ColumnName),
                    PropertyName = ConvertToPascalCase(c.ColumnName),
                    ColumnComment = c.ColumnComment,
                    DbType = c.DbType,
                    CSharpType = c.CSharpType,
                    JavaType = c.JavaType,
                    IsNullable = c.IsNullable,
                    IsKey = c.IsKey,
                    IsIdentity = c.IsIdentity
                }).ToList()
            };

            foreach (var template in templates)
            {
                var renderedContent = RenderTemplate(template.Content, context);
                var filePath = RenderTemplate(template.FilePath, context);
                var fileName = Path.GetFileName(filePath) ?? className;

                resultFiles.Add(new CodeGenFileDto
                {
                    FileName = fileName,
                    FilePath = filePath,
                    Content = renderedContent,
                    FileExtension = Path.GetExtension(filePath) ?? string.Empty
                });
            }
        }

        var result = new CodeGenResultDto
        {
            TaskId = taskId,
            Files = resultFiles
        };

        GeneratedCodeCache[taskId] = result;
        return result;
    }

    public async Task<byte[]> DownloadFileAsync(string taskId, string fileName)
    {
        if (!GeneratedCodeCache.TryGetValue(taskId, out var result))
        {
            throw new Exception("生成任务不存在或已过期");
        }

        var file = result.Files.FirstOrDefault(f => f.FileName == fileName);
        if (file == null)
        {
            throw new Exception("文件不存在");
        }

        return await Task.FromResult(Encoding.UTF8.GetBytes(file.Content));
    }

    public async Task<byte[]> DownloadPackageAsync(string taskId)
    {
        if (!GeneratedCodeCache.TryGetValue(taskId, out var result))
        {
            throw new Exception("生成任务不存在或已过期");
        }

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var file in result.Files)
            {
                var entryPath = file.FilePath.Replace('\\', '/');
                var entry = archive.CreateEntry(entryPath);
                using var entryStream = entry.Open();
                using var writer = new StreamWriter(entryStream, Encoding.UTF8);
                await writer.WriteAsync(file.Content);
            }
        }

        return memoryStream.ToArray();
    }

    #endregion

    #region 私有方法

    private DbConnection CreateDbConnection(DbConnectionConfigEntity entity)
    {
        var connectionString = !string.IsNullOrWhiteSpace(entity.ConnectionString)
            ? entity.ConnectionString
            : BuildConnectionString(entity);

        return entity.DbType switch
        {
            CodeGenDbType.MySql => new MySqlConnection(connectionString),
            CodeGenDbType.SqlServer => new System.Data.SqlClient.SqlConnection(connectionString),
            CodeGenDbType.PostgreSql => new NpgsqlConnection(connectionString),
            _ => throw new NotSupportedException($"不支持的数据库类型: {entity.DbType}")
        };
    }

    private string BuildConnectionString(DbConnectionConfigEntity entity)
    {
        var password = DecryptPassword(entity.Password);
        return entity.DbType switch
        {
            CodeGenDbType.MySql => $"Server={entity.Host};Port={entity.Port};Database={entity.Database};Uid={entity.Username};Pwd={password};charset=utf8mb4;",
            CodeGenDbType.SqlServer => $"Server={entity.Host},{entity.Port};Database={entity.Database};User Id={entity.Username};Password={password};",
            CodeGenDbType.PostgreSql => $"Host={entity.Host};Port={entity.Port};Database={entity.Database};Username={entity.Username};Password={password};",
            _ => throw new NotSupportedException($"不支持的数据库类型: {entity.DbType}")
        };
    }

    private string EncryptPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return password;
        }
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
    }

    private string DecryptPassword(string encryptedPassword)
    {
        if (string.IsNullOrWhiteSpace(encryptedPassword))
        {
            return encryptedPassword;
        }
        return Encoding.UTF8.GetString(Convert.FromBase64String(encryptedPassword));
    }

    private async Task<List<string>> GetTablesAsync(DbConnection connection, CodeGenDbType dbType)
    {
        var sql = dbType switch
        {
            CodeGenDbType.MySql => "SHOW TABLES",
            CodeGenDbType.SqlServer => "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'",
            CodeGenDbType.PostgreSql => "SELECT tablename FROM pg_tables WHERE schemaname = 'public'",
            _ => throw new NotSupportedException($"不支持的数据库类型: {dbType}")
        };

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        using var reader = await command.ExecuteReaderAsync();

        var tables = new List<string>();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }
        return tables;
    }

    private async Task<string> GetTableCommentAsync(DbConnection connection, CodeGenDbType dbType, string tableName)
    {
        var sql = dbType switch
        {
            CodeGenDbType.MySql => $"SELECT TABLE_COMMENT FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{tableName}'",
            CodeGenDbType.SqlServer => $"SELECT VALUE FROM fn_listextendedproperty('MS_Description', 'SCHEMA', 'dbo', 'TABLE', '{tableName}', NULL, NULL)",
            CodeGenDbType.PostgreSql => $"SELECT obj_description('{tableName}'::regclass)",
            _ => throw new NotSupportedException($"不支持的数据库类型: {dbType}")
        };

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        var result = await command.ExecuteScalarAsync();
        return result?.ToString() ?? tableName;
    }

    private async Task<List<DbColumnInfoDto>> GetColumnsAsync(DbConnection connection, CodeGenDbType dbType, string tableName)
    {
        var sql = dbType switch
        {
            CodeGenDbType.MySql => $@"
                SELECT 
                    COLUMN_NAME as ColumnName,
                    COLUMN_COMMENT as ColumnComment,
                    DATA_TYPE as DataType,
                    IS_NULLABLE as IsNullable,
                    COLUMN_KEY as ColumnKey,
                    EXTRA as Extra
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{tableName}'
                ORDER BY ORDINAL_POSITION",
            CodeGenDbType.SqlServer => $@"
                SELECT 
                    c.name as ColumnName,
                    ISNULL(p.value, '') as ColumnComment,
                    t.name as DataType,
                    c.is_nullable as IsNullable,
                    ISNULL(i.is_primary_key, 0) as IsPrimaryKey,
                    c.is_identity as IsIdentity
                FROM sys.columns c
                INNER JOIN sys.tables tb ON c.object_id = tb.object_id
                INNER JOIN sys.types t ON c.system_type_id = t.system_type_id
                LEFT JOIN sys.extended_properties p ON p.major_id = c.object_id AND p.minor_id = c.column_id AND p.name = 'MS_Description'
                LEFT JOIN sys.index_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                LEFT JOIN sys.indexes i ON i.object_id = ic.object_id AND i.index_id = ic.index_id AND i.is_primary_key = 1
                WHERE tb.name = '{tableName}'
                ORDER BY c.column_id",
            CodeGenDbType.PostgreSql => $@"
                SELECT 
                    a.attname as ColumnName,
                    d.description as ColumnComment,
                    t.typname as DataType,
                    a.attnotnull as IsNullable,
                    (SELECT EXISTS(SELECT 1 FROM pg_constraint WHERE contype='p' AND conrelid=a.attrelid AND a.attnum=ANY(conkey))) as IsPrimaryKey,
                    pg_get_serial_sequence('{tableName}', a.attname) IS NOT NULL as IsIdentity
                FROM pg_attribute a
                INNER JOIN pg_class c ON a.attrelid = c.oid
                INNER JOIN pg_type t ON a.atttypid = t.oid
                LEFT JOIN pg_description d ON d.objoid = a.attrelid AND d.objsubid = a.attnum
                WHERE c.relname = '{tableName}' AND a.attnum > 0 AND NOT a.attisdropped
                ORDER BY a.attnum",
            _ => throw new NotSupportedException($"不支持的数据库类型: {dbType}")
        };

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        using var reader = await command.ExecuteReaderAsync();

        var columns = new List<DbColumnInfoDto>();
        while (await reader.ReadAsync())
        {
            var columnName = reader.GetString(0);
            var columnComment = reader.IsDBNull(1) ? "" : reader.GetString(1);
            var dataType = reader.IsDBNull(2) ? "" : reader.GetString(2);

            bool isNullable;
            bool isKey;
            bool isIdentity;

            switch (dbType)
            {
                case CodeGenDbType.MySql:
                    {
                        var isNullableStr = reader.IsDBNull(3) ? "YES" : reader.GetString(3);
                        var columnKey = reader.IsDBNull(4) ? "" : reader.GetString(4);
                        var extra = reader.IsDBNull(5) ? "" : reader.GetString(5);

                        isNullable = isNullableStr == "YES";
                        isKey = columnKey == "PRI";
                        isIdentity = extra.Contains("auto_increment");
                        break;
                    }
                case CodeGenDbType.SqlServer:
                    {
                        isNullable = reader.GetBoolean(3);
                        isKey = reader.GetBoolean(4);
                        isIdentity = reader.GetBoolean(5);
                        isNullable = !isNullable;
                        break;
                    }
                default:
                    {
                        isNullable = reader.GetBoolean(3);
                        isKey = reader.GetBoolean(4);
                        isIdentity = reader.GetBoolean(5);
                        break;
                    }
            }

            columns.Add(new DbColumnInfoDto
            {
                ColumnName = columnName,
                ColumnComment = string.IsNullOrWhiteSpace(columnComment) ? columnName : columnComment,
                DbType = dataType,
                CSharpType = ConvertToCSharpType(dataType, isNullable),
                JavaType = ConvertToJavaType(dataType),
                IsNullable = isNullable,
                IsKey = isKey,
                IsIdentity = isIdentity
            });
        }
        return columns;
    }

    private async Task<DbTableInfoDto> GetTableInfoAsync(DbConnection connection, CodeGenDbType dbType, string tableName)
    {
        var columns = await GetColumnsAsync(connection, dbType, tableName);
        var tableComment = await GetTableCommentAsync(connection, dbType, tableName);

        return new DbTableInfoDto
        {
            TableName = tableName,
            TableComment = tableComment,
            Columns = columns
        };
    }

    private string ConvertToCSharpType(string dbType, bool isNullable)
    {
        var type = dbType.ToLower() switch
        {
            "bigint" => "long",
            "int" => "int",
            "integer" => "int",
            "smallint" => "short",
            "tinyint" => "byte",
            "decimal" => "decimal",
            "numeric" => "decimal",
            "float" => "float",
            "double" => "double",
            "bit" => "bool",
            "boolean" => "bool",
            "datetime" => "DateTime",
            "timestamp" => "DateTime",
            "date" => "DateTime",
            "time" => "TimeSpan",
            "char" => "string",
            "varchar" => "string",
            "text" => "string",
            "longtext" => "string",
            "nvarchar" => "string",
            "ntext" => "string",
            "binary" => "byte[]",
            "varbinary" => "byte[]",
            "blob" => "byte[]",
            _ => "string"
        };

        if (isNullable && type != "string" && type != "byte[]")
        {
            return $"{type}?";
        }
        return type;
    }

    private string ConvertToJavaType(string dbType)
    {
        return dbType.ToLower() switch
        {
            "bigint" => "Long",
            "int" => "Integer",
            "integer" => "Integer",
            "smallint" => "Integer",
            "tinyint" => "Integer",
            "decimal" => "BigDecimal",
            "numeric" => "BigDecimal",
            "float" => "Float",
            "double" => "Double",
            "bit" => "Boolean",
            "boolean" => "Boolean",
            "datetime" => "Date",
            "timestamp" => "Date",
            "date" => "Date",
            "time" => "Date",
            "char" => "String",
            "varchar" => "String",
            "text" => "String",
            "longtext" => "String",
            "nvarchar" => "String",
            "ntext" => "String",
            "binary" => "byte[]",
            "varbinary" => "byte[]",
            "blob" => "byte[]",
            _ => "String"
        };
    }

    private string ConvertToPascalCase(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        var words = str.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join("", words.Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }

    private string ConvertToCamelCase(string str)
    {
        var pascalCase = ConvertToPascalCase(str);
        return string.IsNullOrWhiteSpace(pascalCase) ? pascalCase : char.ToLower(pascalCase[0]) + pascalCase.Substring(1);
    }

    private string RemovePrefix(string str, string prefix)
    {
        if (string.IsNullOrWhiteSpace(str) || string.IsNullOrWhiteSpace(prefix))
        {
            return str;
        }

        if (str.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return str.Substring(prefix.Length);
        }

        return str;
    }

    private string RenderTemplate(string template, object context)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return string.Empty;
        }
        try
        {
            var handlebars = Handlebars.Create();
            
            handlebars.RegisterHelper("or", (output, context, arguments) =>
            {
                foreach (var arg in arguments)
                {
                    if (arg != null)
                    {
                        var value = arg.ToString();
                        if (!string.IsNullOrWhiteSpace(value) && value != "False")
                        {
                            output.Write(value);
                            return;
                        }
                    }
                }
            });

            var compiledTemplate = handlebars.Compile(template);
            var result = compiledTemplate(context);
            
            return System.Net.WebUtility.HtmlDecode(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "模板渲染失败");
            throw new Exception($"模板渲染失败: {ex.Message}");
        }
    }

    #endregion
}
