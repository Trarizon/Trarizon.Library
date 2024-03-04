using Microsoft.CodeAnalysis;
using Trarizon.Library.GeneratorToolkit.Wrappers;

namespace Trarizon.Library.GeneratorToolkit.Factories;
public static class DiagnosticFactory
{
    public static Diagnostic Create(DiagnosticDescriptor descriptor, in Either<SyntaxNode, SyntaxToken> syntax, params object?[]? messageArgs)
    {
        if (syntax.TryGetLeftValue(out var node, out var token))
            return Diagnostic.Create(descriptor, Location.Create(node.SyntaxTree, node.Span), messageArgs);
        else
            return Diagnostic.Create(descriptor, Location.Create(token.SyntaxTree!, token.Span), messageArgs);
    }
}
