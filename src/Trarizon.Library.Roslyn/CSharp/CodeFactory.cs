using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Trarizon.Library.Roslyn.CSharp;
public static class CodeFactory
{
    public static string ClonePartialDeclaration(TypeDeclarationSyntax typeDeclaration)
    {
        var keyword = typeDeclaration is RecordDeclarationSyntax rcd
            ? $"{rcd.Keyword} {rcd.ClassOrStructKeyword}"
            : typeDeclaration.Keyword.ToString();
        return string.Join(" ", "partial", keyword, $"{typeDeclaration.Identifier}{typeDeclaration.TypeParameterList}");
    }

    /// <summary>
    /// <c>#pragma warning disable/restore {err-codes}</c>
    /// </summary>
    public static string PragmaWarningTrivia(string errorCodes, bool enable)
        => $"#pragma warning {(enable ? "restore" : "disable")} {errorCodes}";
}
