using System.Text.RegularExpressions;
using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.CodeGen;

public static class JavaEntityParser
{
    public static CodeFirstParseResultDto Parse(string sourceCode)
    {
        var className = ExtractClassName(sourceCode);
        var packageName = ExtractPackageName(sourceCode);
        var tableComment = ExtractClassComment(sourceCode, className);
        var tableName = ExtractTableName(sourceCode, className);
        var columns = ExtractFields(sourceCode);

        return new CodeFirstParseResultDto
        {
            ClassName = className,
            TableName = tableName,
            TableComment = string.IsNullOrWhiteSpace(tableComment) ? className : tableComment,
            Namespace = packageName,
            Columns = columns
        };
    }

    private static string ExtractClassName(string code)
    {
        var match = Regex.Match(code, @"public\s+class\s+(\w+)");
        if (!match.Success)
        {
            match = Regex.Match(code, @"class\s+(\w+)");
        }
        if (!match.Success)
        {
            throw new InvalidOperationException("未能从源码中解析出类名");
        }
        return match.Groups[1].Value;
    }

    private static string ExtractPackageName(string code)
    {
        var match = Regex.Match(code, @"package\s+([\w.]+)\s*;");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static string ExtractClassComment(string code, string className)
    {
        var pattern = $@"/\*\*\s*\n?\s*\*\s*(.+?)\s*\n.*?\*/\s*\n\s*public\s+class\s+{className}";
        var match = Regex.Match(code, pattern, RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    private static string ExtractTableName(string code, string className)
    {
        var match = Regex.Match(code, @"@Table\s*\(\s*name\s*=\s*""(\w+)""");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return ConvertToSnakeLower(className);
    }

    private static List<CodeGenColumnConfigDto> ExtractFields(string code)
    {
        var columns = new List<CodeGenColumnConfigDto>();

        var fieldRegex = new Regex(
            @"(?://\s*(.+?)\s*\n\s*)?(?:\s*@\w+(?:\([^)]*\))?\s*\n\s*)*private\s+(\w+(?:<\w+>)?)\s+(\w+)\s*;",
            RegexOptions.Multiline);

        foreach (Match match in fieldRegex.Matches(code))
        {
            var comment = match.Groups[1].Success ? match.Groups[1].Value.Trim() : string.Empty;
            var javaType = match.Groups[2].Value;
            var fieldName = match.Groups[3].Value;

            if (IsBaseField(fieldName))
            {
                continue;
            }

            var propertyName = char.ToUpper(fieldName[0]) + fieldName[1..];
            var isKey = Regex.IsMatch(code, $@"@Id\s*\n\s*private\s+\w+\s+{fieldName}");
            var isNullable = !javaType.Equals("int", StringComparison.OrdinalIgnoreCase)
                && !javaType.Equals("long", StringComparison.OrdinalIgnoreCase)
                && !javaType.Equals("boolean", StringComparison.OrdinalIgnoreCase);

            columns.Add(new CodeGenColumnConfigDto
            {
                PropertyName = propertyName,
                FieldName = fieldName,
                ColumnName = fieldName,
                ColumnComment = string.IsNullOrWhiteSpace(comment) ? propertyName : comment,
                JavaType = javaType,
                CSharpType = ConvertToCSharpType(javaType),
                IsNullable = isNullable,
                IsKey = isKey,
                IsIdentity = false
            });
        }

        return columns;
    }

    private static bool IsBaseField(string fieldName)
    {
        var baseFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "id", "createUserId", "createTime", "updateUserId", "updateTime", "isDelete"
        };
        return baseFields.Contains(fieldName);
    }

    private static string ConvertToCSharpType(string javaType)
    {
        return javaType switch
        {
            "Long" => "long",
            "long" => "long",
            "Integer" => "int",
            "int" => "int",
            "Short" => "short",
            "short" => "short",
            "Byte" => "byte",
            "byte" => "byte",
            "BigDecimal" => "decimal",
            "Float" => "float",
            "float" => "float",
            "Double" => "double",
            "double" => "double",
            "Boolean" => "bool",
            "boolean" => "bool",
            "Date" => "DateTime",
            "String" => "string",
            "UUID" => "string",
            _ => "string"
        };
    }

    private static string ConvertToSnakeLower(string camelCase)
    {
        return string.Concat(camelCase.Select((ch, i) =>
            char.IsUpper(ch) && i > 0 ? "_" + char.ToLower(ch) : char.ToLower(ch).ToString()
        ));
    }
}