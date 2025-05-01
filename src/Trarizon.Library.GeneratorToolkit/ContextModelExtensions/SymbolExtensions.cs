using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections;
using Trarizon.Library.GeneratorToolkit.CoreLib.Collections;

namespace Trarizon.Library.GeneratorToolkit.ContextModelExtensions;
public static class SymbolExtensions
{
    public static bool MatchDisplayString([NotNullWhen(true)] this ISymbol? symbol, string displayString, SymbolDisplayFormat? symbolDisplayFormat = null)
    {
        if (symbol is null)
            return false;
        return symbol.ToDisplayString(symbolDisplayFormat) == displayString;
    }

    public static bool TryGetGeneratedCodeAttribute(this ISymbol symbol, string? tool, string? version, [NotNullWhen(true)] out AttributeData? attribute)
        => symbol.GetAttributes().TryFirst(attr =>
        {
            if (attr is null)
                return false;
            if (!attr.AttributeClass.MatchDisplayString(Literals.GeneratedCodeAttribute_TypeName))
                return false;
            if (tool is null)
                return true;
            if (tool != attr.GetConstructorArgument<string>(Literals.GeneratedCodeAttribute_Tool_ConstructorIndex).Value)
                return false;
            if (version is null)
                return true;
            if (version != attr.GetConstructorArgument<string>(Literals.GeneratedCodeAttribute_Version_ConstructorIndex).Value)
                return false;
            return true;
        }, out attribute);

    public static bool TryGetGeneratedCodeAttribute(this ISymbol symbol, string tool, [NotNullWhen(true)] out AttributeData? attribute)
        => symbol.TryGetGeneratedCodeAttribute(tool, null, out attribute);

    public static bool TryGetGeneratedCodeAttribute(this ISymbol symbol, [NotNullWhen(true)] out AttributeData? attribute)
        => symbol.TryGetGeneratedCodeAttribute(null, null, out attribute);

    public static bool IsNullableValueType(this ITypeSymbol type, [NotNullWhen(true)] out ITypeSymbol? underlyingType)
    {
        if (type is { IsValueType: true, NullableAnnotation: NullableAnnotation.Annotated }) {
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

        // Value type cannot use .WithNullableAnnotation, get the first generic
        // argument of Nullable<T>
        if (type.IsValueType)
            return ((INamedTypeSymbol)type).TypeArguments[0];

        return type.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
    }

    public static string ToValidFileNameString(this ISymbol symbol)
    {
        var display = symbol.ToDisplayString();
        var builder = (stackalloc char[display.Length]);
        display.AsSpan().CopyTo(builder);
        foreach (ref var c in builder) {
            if (c is '<') c = '{';
            if (c is '>') c = '}';
        }
        return builder.ToString();
    }

    public static string ToFullQualifiedDisplayString(this ISymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    /// <returns><see langword="null"/> if <paramref name="symbol"/> is global namespace</returns>
    public static string? ToNonGlobalDisplayString(this INamespaceSymbol symbol)
    {
        if (symbol.IsGlobalNamespace)
            return null;
        return symbol.ToDisplayString();
    }
}
