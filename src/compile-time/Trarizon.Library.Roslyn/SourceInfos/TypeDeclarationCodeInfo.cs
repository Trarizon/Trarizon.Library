using System.CodeDom.Compiler;
using Trarizon.Library.Roslyn.SourceInfos.Emitting;

namespace Trarizon.Library.Roslyn.SourceInfos;
public record class TypeDeclarationCodeInfo(
    NamespaceDeclarationCodeInfo? Namespace,
    string Keyword,
    string Name)
    : NamespaceOrTypeDeclarationCodeInfo
{
    private TypeDeclarationCodeInfo? _parent;

    public TypeDeclarationCodeInfo? ParentType { get => _parent; init => _parent = value; }

    public sealed override NamespaceOrTypeDeclarationCodeInfo? Parent => ParentType is null ? Namespace : ParentType;

    internal void SetParent(TypeDeclarationCodeInfo parent) => _parent = parent;

    protected internal override IndentedTextWriterExtensions.DeferDedents EmitCSharp(IndentedTextWriter writer)
    {
        writer.WriteLine($"partial {Keyword} {Name}");
        return writer.EnterBracketDedentScope('{');
    }
}
