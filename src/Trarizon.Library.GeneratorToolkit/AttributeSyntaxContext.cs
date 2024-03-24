using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Trarizon.Library.GeneratorToolkit;
public class AttributeSyntaxContext<TSyntax, TSymbol>(GeneratorAttributeSyntaxContext context)
    where TSyntax : SyntaxNode
    where TSymbol : ISymbol
{
    public TSyntax Syntax { get; } = (TSyntax)context.TargetNode;
    public TSymbol Symbol { get; } = (TSymbol)context.TargetSymbol;
    public ImmutableArray<AttributeData> Attributes { get; } = context.Attributes;
    public SemanticModel SemanticModel { get; } = context.SemanticModel;
}
