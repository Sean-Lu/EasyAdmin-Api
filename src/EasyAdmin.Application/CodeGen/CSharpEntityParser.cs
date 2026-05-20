using System.Text.RegularExpressions;
using EasyAdmin.Application.Dtos;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAdmin.Application.CodeGen;

public static class CSharpEntityParser
{
    private static readonly HashSet<string> BasePropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "CreateUserId", "CreateTime", "UpdateUserId", "UpdateTime", "IsDelete",
        "PId", "Sort", "Children", "TenantId"
    };

    public static CodeFirstParseResultDto Parse(string sourceCode)
    {
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetCompilationUnitRoot();

        var classDecl = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classDecl == null)
        {
            throw new InvalidOperationException("未能从源码中解析出类定义");
        }

        var namespaceName = (classDecl.Parent as NamespaceDeclarationSyntax)?.Name.ToString()
            ?? (classDecl.Parent as FileScopedNamespaceDeclarationSyntax)?.Name.ToString()
            ?? string.Empty;

        var rawClassName = classDecl.Identifier.Text;
        var className = RemoveEntitySuffix(rawClassName);
        var tableName = className;
        var tableComment = ExtractSummaryComment(classDecl.GetLeadingTrivia().ToString());

        var tableAttr = classDecl.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(a => a.Name.ToString() is "Table" or "TableAttribute");
        if (tableAttr?.ArgumentList?.Arguments.FirstOrDefault()?.Expression is LiteralExpressionSyntax tableNameLiteral)
        {
            tableName = tableNameLiteral.Token.ValueText;
        }

        var columns = ParseProperties(classDecl);

        return new CodeFirstParseResultDto
        {
            ClassName = className,
            TableName = tableName,
            TableComment = string.IsNullOrWhiteSpace(tableComment) ? className : tableComment,
            Namespace = namespaceName,
            Columns = columns
        };
    }

    private static List<CodeGenColumnConfigDto> ParseProperties(ClassDeclarationSyntax classDecl)
    {
        var columns = new List<CodeGenColumnConfigDto>();
        var processedProps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var propertyDecls = classDecl.Members.OfType<PropertyDeclarationSyntax>();

        foreach (var prop in propertyDecls)
        {
            var propName = prop.Identifier.Text;
            if (BasePropertyNames.Contains(propName))
            {
                continue;
            }
            if (processedProps.Contains(propName))
            {
                continue;
            }
            processedProps.Add(propName);

            var csharpType = GetCSharpType(prop.Type);
            var isNullable = prop.Type is NullableTypeSyntax;
            var isKey = HasAttribute(prop, "Key");
            var description = ExtractSummaryComment(prop.GetLeadingTrivia().ToString());
            var fieldName = char.ToLower(propName[0]) + propName[1..];

            var column = new CodeGenColumnConfigDto
            {
                PropertyName = propName,
                FieldName = fieldName,
                ColumnName = propName,
                ColumnComment = string.IsNullOrWhiteSpace(description) ? propName : description,
                CSharpType = isNullable && csharpType != "string" && csharpType != "byte[]" ? $"{csharpType}?" : csharpType,
                JavaType = ConvertToJavaType(csharpType),
                IsNullable = isNullable,
                IsKey = isKey,
                IsIdentity = HasAttribute(prop, "DatabaseGenerated")
            };

            columns.Add(column);
        }

        return columns;
    }

    private static string GetCSharpType(TypeSyntax typeSyntax)
    {
        var typeStr = typeSyntax.ToString().Trim();
        if (typeSyntax is NullableTypeSyntax nts)
        {
            typeStr = nts.ElementType.ToString().Trim();
        }
        if (typeSyntax is GenericNameSyntax gns)
        {
            typeStr = gns.Identifier.Text;
        }
        return typeStr;
    }

    private static string ConvertToJavaType(string csharpType)
    {
        return csharpType.ToLower() switch
        {
            "long" => "Long",
            "int" => "Integer",
            "integer" => "Integer",
            "short" => "Short",
            "byte" => "Byte",
            "decimal" => "BigDecimal",
            "float" => "Float",
            "double" => "Double",
            "bool" => "Boolean",
            "boolean" => "Boolean",
            "datetime" => "Date",
            "timespan" => "Date",
            "string" => "String",
            "guid" => "String",
            _ => "String"
        };
    }

    private static bool HasAttribute(PropertyDeclarationSyntax prop, string attrName)
    {
        return prop.AttributeLists
            .SelectMany(al => al.Attributes)
            .Any(a => a.Name.ToString() == attrName || a.Name.ToString() == $"{attrName}Attribute");
    }

    private static string RemoveEntitySuffix(string className)
    {
        if (className.EndsWith("Entity", StringComparison.OrdinalIgnoreCase) && className.Length > 6)
        {
            return className[..^6];
        }
        return className;
    }

    private static string ExtractSummaryComment(string triviaText)
    {
        var match = Regex.Match(triviaText, @"<summary>\s*(.*?)\s*</summary>", RegexOptions.Singleline);
        if (match.Success)
        {
            var comment = match.Groups[1].Value.Trim();
            comment = Regex.Replace(comment, @"^\s*///\s*", "", RegexOptions.Multiline);
            comment = Regex.Replace(comment, @"\s+", " ").Trim();
            return comment;
        }
        return string.Empty;
    }
}