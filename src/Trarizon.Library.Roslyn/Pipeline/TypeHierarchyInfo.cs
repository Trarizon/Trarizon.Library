using Microsoft.CodeAnalysis;

namespace Trarizon.Library.Roslyn.Pipeline;
/// <summary>
/// Represents a type or namespace
/// </summary>
public readonly record struct TypeHierarchyInfo
{
    public readonly record struct TypeNode
    {
        public string Keywords { get; }
        public string Name { get; }
        internal TypeNode(string keywords, string name)
        {
            Keywords = keywords;
            Name = name;
        }
    }

    public readonly EquatableReadOnlyMemory<TypeNode> Types { get; }

    private TypeHierarchyInfo(string? @namespace, EquatableReadOnlyMemory<TypeNode> nodes)
    {
        Namespace = @namespace;
        Types = nodes;
    }

    /// <summary>
    /// Namespace of current type, or equals to <see cref="Name"/> for namespace
    /// </summary>
    public string? Namespace { get; }

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
    /// The minimal keywords that you can directly a prepend <c>partial</c> keyword of current type, or "namespace" for namespace
    /// </summary>
    public string Keywords => Types.Length == 0 ? "namespace" : Types.Span[^1].Keywords;

    /// <summary>
    /// Name of current type, or full namespace
    /// </summary>
    public string Name => Types.Length == 0 ? (Namespace ?? "") : Types.Span[^1].Name;

    public bool IsNamespace => Types.Length == 0;

    private static readonly SymbolDisplayFormat TypeNameFormat = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints);

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
                    // { IsUnion: true } => "union",
                    _ => "/* unknown type kind */class",
                };

                return new TypeNode(
                    Fmt(null, stackalloc char[16], $"{@record}{keyword}"),
                    type.ToDisplayString(TypeNameFormat)
                );
            })
            .ToArray();
        Array.Reverse(type);
        return new TypeHierarchyInfo(nsName, type);
    }
}