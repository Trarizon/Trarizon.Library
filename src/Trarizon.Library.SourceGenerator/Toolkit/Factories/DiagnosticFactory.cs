using Microsoft.CodeAnalysis;

namespace Trarizon.Library.SourceGenerator.Toolkit.Factories;
public static class DiagnosticFactory
{
    public static Diagnostic Create(DiagnosticDescriptor descriptor, in SyntaxNodeOrToken syntax, params object?[]? messageArgs)
        => Diagnostic.Create(descriptor, Location.Create(syntax.SyntaxTree!, syntax.Span), messageArgs);
}
