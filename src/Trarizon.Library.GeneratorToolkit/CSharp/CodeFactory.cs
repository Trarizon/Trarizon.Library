using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Trarizon.Library.GeneratorToolkit.CSharp;
/// <summary>
/// Extensions of <see cref="SyntaxFactory"/>,
/// </summary>
public static class CodeFactory
{
    public static string GeneratedCodeAttribute(string tool, string version)
        => $@"global::{Literals.GeneratedCodeAttribute_TypeName}(""{tool}"", ""{version}"")";

    public static string InterceptsLocationAttribute(string filePath, int line, int column)
        => $@"global::{Literals.InterceptsLocationAttribute_TypeName}(@""{filePath}"", {line}, {column})";

    public static string InterceptsLocationAttribute((string FilePath, int Line, int Column) location)
        => InterceptsLocationAttribute(location.FilePath, location.Line, location.Column);

    public static string FileScopeInterceptsLocationAttributeDeclaration => """
        namespace System.Runtime.CompilerServices
        {
            [global::System.AttributeUsageAttribute(AttributeTargets.Method, AllowMultiple = true)]
            [global::System.Diagnostics.Conditional("CODE_ANALYSIS")]
            file sealed class InterceptsLocationAttribute(string filePath, int line, int character) : global::System.Attribute;
        }
        """;

    public static string ClonePartialDeclaration(TypeDeclarationSyntax typeDeclaration)
    {
        var keyword = typeDeclaration is RecordDeclarationSyntax rcd
            ? $"{rcd.Keyword} {rcd.ClassOrStructKeyword}"
            : typeDeclaration.Keyword.ToString();
        return $"partial {keyword} {typeDeclaration.Identifier}{typeDeclaration.TypeParameterList}";
    }
}
