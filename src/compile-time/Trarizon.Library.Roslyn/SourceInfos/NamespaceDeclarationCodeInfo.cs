using System.CodeDom.Compiler;
using Trarizon.Library.Roslyn.SourceInfos.Emitting;

namespace Trarizon.Library.Roslyn.SourceInfos;
public sealed record class NamespaceDeclarationCodeInfo(
    string Name)
    : NamespaceOrTypeDeclarationCodeInfo
{
    /// <summary>
    /// Always null
    /// </summary>
    public override NamespaceOrTypeDeclarationCodeInfo? Parent => null;

    protected internal override IndentedTextWriterExtensions.DeferDedents EmitCSharp(IndentedTextWriter writer)
    {
        writer.WriteLine($"namespace {Name}");
        return writer.EnterBracketDedentScope('{');
    }
}
