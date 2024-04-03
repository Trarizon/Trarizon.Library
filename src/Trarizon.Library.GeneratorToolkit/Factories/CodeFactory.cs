using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Trarizon.Library.GeneratorToolkit.Factories;
/// <summary>
/// Extensions of <see cref="SyntaxFactory"/>,
/// </summary>
public static class CodeFactory
{
    public static string GeneratedCodeAttribute(string tool, string version)
    {
        return $@"{GlobalLiterals.GeneratedCodeAttribute_TypeName}(""{tool}"", ""{version}"")";
    }

    public static string ClonePartialDeclaration(TypeDeclarationSyntax typeDeclaration)
    {
        var keyword = typeDeclaration is RecordDeclarationSyntax rcd
            ? $"{rcd.Keyword} {rcd.ClassOrStructKeyword}"
            : typeDeclaration.Keyword.ToString();
        return $"partial {keyword} {typeDeclaration.Identifier}{typeDeclaration.TypeParameterList}";
    }
}
