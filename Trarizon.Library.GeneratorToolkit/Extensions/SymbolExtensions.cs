using Microsoft.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class SymbolExtensions
{
    public static bool HasGeneratedCodeAttribute(this ISymbol symbol)
        => symbol.GetAttributes().Any(attr => attr.AttributeClass?.ToDisplayString() == Literals.GeneratedCodeAttributeFullName);
}
