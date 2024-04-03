using Microsoft.CodeAnalysis;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class SymbolExtensions
{
    public static bool MatchDisplayString([NotNullWhen(true)] this ISymbol? symbol, string displayString, SymbolDisplayFormat? symbolDisplayFormat = null)
        => symbol?.ToDisplayString(symbolDisplayFormat) == displayString;

    public static bool TryGetGeneratedCodeAttribute(this ISymbol symbol, [NotNullWhen(true)] out AttributeData? attribute)
        => symbol.GetAttributes().TryFirst(attr => attr!.AttributeClass.MatchDisplayString(GlobalLiterals.GeneratedCodeAttribute_TypeName), out attribute);

    public static bool TryGetGeneratedCodeAttribute(this ISymbol symbol, [NotNullWhen(true)] out AttributeData? attribute, string tool, string? version = null)
        => symbol.GetAttributes().TryFirst(attr =>
        {
            if (!attr!.AttributeClass.MatchDisplayString(GlobalLiterals.GeneratedCodeAttribute_TypeName))
                return false;
            if (tool != attr.GetConstructorArgument<string>(GlobalLiterals.GeneratedCodeAttribute_Tool_ConstructorIndex).Value)
                return false;
            if (version is not null && version != attr.GetConstructorArgument<string>(GlobalLiterals.GeneratedCodeAttribute_Version_ConstructorIndex).Value)
                return false;
            return true;
        }, out attribute);

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

    public static string ToValidFileNameString(this ISymbol symbol)
    {
        var display = symbol.ToDisplayString();
        var builder = (stackalloc char[display.Length]);
        display.AsSpan().CopyTo(builder);
        foreach (ref var c in builder) {
            if (c is '<')
                c = '{';
            else if (c is '>')
                c = '}';
        }
        return builder.ToString();
    }

    public static string ToFullQualifiedDisplayString(this ISymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public static string? ToNonGlobalDisplayString(this INamespaceSymbol @namespace)
    {
        if (@namespace.IsGlobalNamespace)
            return null;

        return @namespace.ToDisplayString();
    }
}
