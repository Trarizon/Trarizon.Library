using Microsoft.CodeAnalysis;

namespace Trarizon.Library.Roslyn.Pipeline;
/// <summary>
/// Represents a type or namespace
/// </summary>
public readonly record struct TypeHierarchyInfo
{
    public readonly record struct TypeNode(string Keyword, string Name);

    public readonly EquatableReadOnlyMemory<TypeNode> Types { get; private init; }

    private TypeHierarchyInfo(string? @namespace, EquatableReadOnlyMemory<TypeNode> nodes)
    {
        Namespace = @namespace;
        Types = nodes;
    }

    /// <summary>
    /// Namespace of current type, or equals to <see cref="Name"/> for namespace
    /// </summary>
    public string? Namespace { get; internal init; }

    /// <summary>
    /// Parent hierarchy, null for namespace or types in global namespace
    /// </summary>
    public TypeHierarchyInfo Parent
    {
        get
        {
            if (Types.Length == 0)
                return default;
            return new(Namespace, Types[..^1]);
        }
    }

    /// <summary>
    /// Keyword of current type, or equals to <see cref="Name"/> for namespace
    /// </summary>
    public string Keyword => Types.Length == 0 ? "namespace" : Types.Span[^1].Keyword;

    /// <summary>
    /// Name of current type, or equals to <see cref="Keyword"/> for namespace
    /// </summary>
    public string Name => Types.Length == 0 ? (Namespace ?? "") : Types.Span[^1].Name;

    public bool IsNamespace => Types.Length == 0;

    public static TypeHierarchyInfo Create(INamedTypeSymbol symbol)
    {
        var ns = symbol.ContainingNamespace;
        var nsName = ns.IsGlobalNamespace ? null : ns.ToString();
        var type = symbol
            .ContainingTypes(includeSelf: true)
            .Select(type =>
            {
                var @record = type.IsRecord ? "record " : "";
                var keyword = type switch
                {
                    { TypeKind: TypeKind.Class } => "class",
                    { TypeKind: TypeKind.Struct } => "struct",
                    { TypeKind: TypeKind.Interface } => "interface",
                    { TypeKind: TypeKind.Enum } => "enum",
                    _ => "/* unknown type kind */",
                };

                var typeParameters = type.TypeParameters.Length == 0 ? "" : $"<{string.Join(", ", type.TypeParameters.Select(x => x.Name))}>";
                return new TypeNode($"{@record}{keyword}", $"{type.Name}{typeParameters}");
            })
            .ToArray();
        Array.Reverse(type);
        return new TypeHierarchyInfo(nsName, type.ToEquatableImmutableArray());
    }
}