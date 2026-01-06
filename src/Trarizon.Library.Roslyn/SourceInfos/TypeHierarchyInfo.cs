using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.Library.Linq;

namespace Trarizon.Library.Roslyn.SourceInfos;
/// <summary>
/// Represents a type or namespace
/// </summary>
public sealed record class TypeHierarchyInfo
{
    internal TypeHierarchyInfo(string? @namespace, string keyword, string name)
    {
        Namespace = @namespace;
        Keywords = keyword;
        Name = name;
    }

    /// <summary>
    /// Namespace of current type, or equals to <see cref="Name"/> for namespace
    /// </summary>
    public string? Namespace { get; internal init; }

    /// <summary>
    /// Keyword("<see langword="class"/>", "<see langword="record"/> <see langword="struct"/>", etc.) of type, or "<see langword="namespace"/>" for namespace
    /// </summary>
    public string Keywords { get; internal init; }

    /// <summary>
    /// Name of type or namespace, note it is the full name for namespace
    /// </summary>
    public string Name { get; internal init; }

    /// <summary>
    /// Parent hierarchy, null for namespace or types in global namespace
    /// </summary>
    public TypeHierarchyInfo? Parent { get; internal set; }

    public static TypeHierarchyInfo Create(ITypeSymbol symbol, TypeDeclarationSyntax syntax)
    {
        var ns = symbol.ContainingNamespace;
        var nsName = ns.IsGlobalNamespace ? null : ns.ToString();

        var types = syntax
            .AncestorsAndSelf()
            .OfTypeWhile<TypeDeclarationSyntax>()
            .Select(type =>
            {
                var keyword = type is RecordDeclarationSyntax rcd
                    ? $"{rcd.Keyword} {rcd.ClassOrStructKeyword}"
                    : type.Keyword.ToString();
                return new TypeHierarchyInfo(nsName, keyword, $"{type.Identifier}{type.TypeParameterList}");
            })
            .ToArray();

        TypeHierarchyInfo res = types.First();
        foreach (var (l, r) in types.Adjacent()) {
            l.Parent = r;
        }
        return res;
    }
}
