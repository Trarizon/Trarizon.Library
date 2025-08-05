using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Trarizon.Library.Collections;

namespace Trarizon.Library.Roslyn.SourceInfos;
public static class CodeInfoFactory
{
    /// <returns>
    /// Containing type or namespace, null if given symbol has no containing type and namespace
    /// </returns>
    public static NamespaceOrTypeDeclarationCodeInfo? GetContainingTypeOrNamespaceDeclaration(ISymbol symbol, SyntaxNode syntax)
    {
        var ns = symbol.ContainingNamespace;
        var nsInfo = ns.IsGlobalNamespace ? null : new NamespaceDeclarationCodeInfo(ns.ToString());

        var types = syntax
            .Ancestors()
            .OfTypeWhile<TypeDeclarationSyntax>()
            .Select(type => TypeDeclarationCodeInfoSelector(type, nsInfo));

        TypeDeclarationCodeInfo? first = types.FirstOrDefault();
        if (first is null)
            return nsInfo;

        TypeDeclarationCodeInfo l = first;
        foreach (var r in types.Skip(1)) {
            l.SetParent(r);
            l = r;
        }
        return first;
    }

    public static TypeDeclarationCodeInfo GetTypeDeclaration(ITypeSymbol symbol, TypeDeclarationSyntax syntax)
    {
        var ns = symbol.ContainingNamespace;
        var nsInfo = ns.IsGlobalNamespace ? null : new NamespaceDeclarationCodeInfo(ns.ToString());

        var types = syntax
            .AncestorsAndSelf()
            .OfTypeWhile<TypeDeclarationSyntax>()
            .Select(type => TypeDeclarationCodeInfoSelector(type, nsInfo));

        TypeDeclarationCodeInfo first = types.First();

        TypeDeclarationCodeInfo l = first;
        foreach (var r in types.Skip(1)) {
            l.SetParent(r);
            l = r;
        }

        return first;
    }

    private static TypeDeclarationCodeInfo TypeDeclarationCodeInfoSelector(TypeDeclarationSyntax syntax, NamespaceDeclarationCodeInfo? @namespace)
    {
        var keyword = syntax is RecordDeclarationSyntax rcd
            ? $"{rcd.Keyword} {rcd.ClassOrStructKeyword}"
            : syntax.Keyword.ToString();
        return new TypeDeclarationCodeInfo(@namespace, keyword, $"{syntax.Identifier}{syntax.TypeParameterList}");
    }
}
