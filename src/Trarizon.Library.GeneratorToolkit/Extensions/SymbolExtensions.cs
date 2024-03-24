using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class SymbolExtensions
{
    public static bool MatchDisplayString(this ISymbol symbol, string displayString, SymbolDisplayFormat? symbolDisplayFormat = null)
        => symbol.ToDisplayString(symbolDisplayFormat) == displayString;

    public static bool HasGeneratedCodeAttribute(this ISymbol symbol)
        => symbol.GetAttributes().Any(attr => attr.AttributeClass?.MatchDisplayString(Literals.GeneratedCodeAttributeFullName) == true);

    public static bool IsNullableValueType(this ITypeSymbol type, [NotNullWhen(true)] out ITypeSymbol? underlyingType)
    {
        if (type is {
            IsValueType: true,
            NullableAnnotation: NullableAnnotation.Annotated,
        }) {
            underlyingType = type.RemoveNullableAnnotation();
            return true;
        }
        underlyingType = default;
        return false;
    }

    public static ITypeSymbol RemoveNullableAnnotation(this ITypeSymbol type)
    {
        if (type.NullableAnnotation is not NullableAnnotation.Annotated)
            return type;
        if (type.IsValueType)
            return ((INamedTypeSymbol)type).TypeArguments[0]; // Value type annot use .WithNullableAnnotation
        return type.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
    }
}
