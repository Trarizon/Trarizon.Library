using System.CodeDom.Compiler;
using Trarizon.Library.Roslyn.SourceInfos.Emitting;

namespace Trarizon.Library.Roslyn.SourceInfos;
public abstract record class NamespaceOrTypeDeclarationCodeInfo
{
    /// <summary>
    /// For type, this is the containg type, or namespace if there is no containing type;
    /// <br/>
    /// For namespace, this is null
    /// </summary>
    public abstract NamespaceOrTypeDeclarationCodeInfo? Parent { get; }

    protected internal abstract IndentedTextWriterExtensions.DeferDedents EmitCSharp(IndentedTextWriter writer);
}
