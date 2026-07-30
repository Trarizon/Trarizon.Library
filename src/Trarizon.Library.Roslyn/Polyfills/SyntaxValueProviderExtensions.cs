#if !LATEST_ROSLYN

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
                return default;

            var attrs = symbol.GetAttributeDatasByFullyQualifiedMetadataName(fullyQualifiedMetadataName);
            if (attrs.Length == 0)
                return default;

            return new Optional<T>(transform(new(context.Node, symbol, context.SemanticModel, attrs), cancellationToken));
        }).Where(x => x.HasValue).Select((x, c) => x.Value);
    }

}

#endif
