using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Roslyn.SourceInfos;
/// <summary>
/// Represents a type or namespace
/// </summary>
public record class TypeHierarchyInfo
{
    internal TypeHierarchyInfo(string? @namespace, string keyword, string name)
    {
        Namespace = @namespace;
        Keywords = keyword;
        Name = name;
    }

    internal static TypeHierarchyInfo FromNamespace(string @namespace) => new(@namespace, "namespace", @namespace);

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

    [MemberNotNullWhen(true, nameof(Namespace))]
    public bool IsNamespace => Keywords == "namespace";
}
