using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Trarizon.Library.GeneratorToolkit;
public interface IAttributeSyntaxContext<out TSyntax, out TSymbol>
    where TSyntax : CSharpSyntaxNode
    where TSymbol : ISymbol
{
    /// <summary>
    /// The syntax node the attribute is attached to.  For example, with <c>[CLSCompliant] class C { }</c> this would
    /// the class declaration node.
    /// </summary>
    public TSyntax Syntax { get; }

    /// <summary>
    /// The symbol that the attribute is attached to.  For example, with <c>[CLSCompliant] class C { }</c> this would be
    /// the <see cref="INamedTypeSymbol"/> for <c>"C"</c>.
    /// </summary>
    public TSymbol Symbol { get; }

    /// <summary>
    /// Semantic model for the file that <see cref="Syntax"/> is contained within.
    /// </summary>
    public SemanticModel SemanticModel { get; }

    /// <summary>
    /// <see cref="AttributeData"/>s for any matching attributes on <see cref="Symbol"/>.  Always non-empty.  All
    /// these attributes will have an <see cref="AttributeData.AttributeClass"/> whose fully qualified name metadata
    /// name matches the name requested in <see cref="SyntaxValueProvider.ForAttributeWithMetadataName{T}"/>.
    /// <para>
    /// To get the entire list of attributes, use <see cref="TSymbol.GetAttributes"/> on <see cref="Symbol"/>.
    /// </para>
    /// </summary>
    public ImmutableArray<AttributeData> Attributes { get; }
}

// TODO:constructor
internal sealed class AttributeSyntaxContext<TSyntax, TSymbol>(in GeneratorAttributeSyntaxContext context) : IAttributeSyntaxContext<TSyntax, TSymbol>
    where TSyntax : CSharpSyntaxNode
    where TSymbol : ISymbol
{
    public TSyntax Syntax { get; }

    public TSymbol Symbol => throw new System.NotImplementedException();

    public SemanticModel SemanticModel => throw new System.NotImplementedException();

    public ImmutableArray<AttributeData> Attributes => throw new System.NotImplementedException();
}
