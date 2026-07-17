#if !LATEST_ROSLYN

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Trarizon.Library.Functional;
using Trarizon.Library.Roslyn;

namespace Microsoft.CodeAnalysis;

public static class SyntaxValueProviderExtensions
{

    public static IncrementalValuesProvider<T> ForAttributeWithMetadataName<T>(this SyntaxValueProvider provider,
        string fullyQualifiedMetadataName,
        Func<SyntaxNode, CancellationToken, bool> predicate,
        Func<GeneratorAttributeSyntaxContext, CancellationToken, T> transform)
    {
        return provider.CreateSyntaxProvider(predicate, (context, cancellationToken) =>
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken);
            if (symbol == null)
                return Optional.None;

            var attrs = symbol.GetAttributeDatasByFullyQualifiedMetadataName(fullyQualifiedMetadataName);
            if (attrs.Length == 0)
                return Optional.None;

            return Optional.Of(transform(new(context.Node, symbol, context.SemanticModel, attrs), cancellationToken));
        }).Where(x => x.HasValue).Select((x, c) => x.Value);
    }

}

#endif
