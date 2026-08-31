#if !LATEST_ROSLYN

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.Library.Roslyn;

namespace Microsoft.CodeAnalysis;

public static class SyntaxValueProviderExtensions
{
    public static IncrementalValuesProvider<T> ForAttributeWithMetadataName<T>(this SyntaxValueProvider provider,
        string fullyQualifiedMetadataName,
        Func<SyntaxNode, CancellationToken, bool> predicate,
        Func<GeneratorAttributeSyntaxContext, CancellationToken, T> transform)
    {
        return provider.CreateSyntaxProvider(
            (node, cancellationToken) =>
            {
                if(node is MemberDeclarationSyntax member)
                    return member.AttributeLists.Count > 0 && predicate(member, cancellationToken);
                if (node is BaseParameterSyntax parameter)
                    return parameter.AttributeLists.Count > 0 && predicate(parameter, cancellationToken);
                if (node is TypeParameterSyntax typeParameter)
                    return typeParameter.AttributeLists.Count > 0 && predicate(typeParameter, cancellationToken);

                // Maybe there's some node types i haven't considered
                return predicate(node, cancellationToken);
            },
            (context, cancellationToken) =>
            {
                var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken);
                if (symbol == null)
                    return default;

                var attrs = symbol.GetAttributeDatasByFullyQualifiedMetadataName(fullyQualifiedMetadataName);
                if (attrs.Length == 0)
                    return default;

                return new Optional<T>(transform(new(context.Node, symbol, context.SemanticModel, attrs), cancellationToken));
            })
            .Where(x => x.HasValue)
            .Select((x, c) => x.Value);
    }

}

#endif
