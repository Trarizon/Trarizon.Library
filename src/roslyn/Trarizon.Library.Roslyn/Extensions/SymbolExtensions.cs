using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.Library.Collections;

namespace Trarizon.Library.Roslyn.Extensions;
public static class SymbolExtensions
{
    [field: MaybeNull]
    public static IEqualityComparer<ISymbol?> OriginalDefinationEqualityComparer => field ??= TraComparison.CreateEqualityComparer<ISymbol?>(
        (x, y) => SymbolEqualityComparer.Default.Equals(x?.OriginalDefinition, y?.OriginalDefinition),
        obj => SymbolEqualityComparer.Default.GetHashCode(obj?.OriginalDefinition));

    /// <remarks>
    /// eg: <c>List&lt;T></c> as <c>System.Collections.Generic.List</c>
    /// </remarks>
    [field: MaybeNull]
    public static SymbolDisplayFormat DefaultDisplayFormatWithouGeneric => field ??= SymbolDisplayFormat.CSharpErrorMessageFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None);

    #region Display String

    public static bool MatchDisplayString([NotNullWhen(true)] this ISymbol? symbol, string displayString, SymbolDisplayFormat? symbolDisplayFormat = null)
    {
        if (symbol is null)
            return false;
        return symbol.ToDisplayString(symbolDisplayFormat) == displayString;
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

    /// <summary>
    /// Return the full qualified display string, include prefix <c>global::</c>
    /// </summary>
    public static string ToFullQualifiedDisplayString(this ISymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    /// <returns><see langword="null"/> if <paramref name="symbol"/> is global namespace</returns>
    public static string? ToNonGlobalDisplayString(this INamespaceSymbol symbol)
    {
        if (symbol.IsGlobalNamespace)
            return null;
        return symbol.ToDisplayString();
    }

    #endregion

    #region Attribute

    public static bool TryGetAttributeData([NotNullWhen(true)] this ISymbol? symbol, string attributeTypeFullName, [NotNullWhen(true)] out AttributeData? attribute)
    {
        if (symbol is null) {
            attribute = null;
            return false;
        }
        attribute = symbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass.MatchDisplayString(attributeTypeFullName));
        return attribute is not null;
    }

    public static bool TryGetGeneratedCodeAttribute(this ISymbol symbol, string? tool, string? version, [NotNullWhen(true)] out AttributeData? attribute)
    {
        attribute = null;

        if (!KnownLiterals.GeneratedCodeAttributeProxy.TryGet(symbol, out var attrProxy))
            return false;
        if (tool is not null && tool != attrProxy.Tool)
            return false;
        if (version is not null && version != attrProxy.Version)
            return false;

        attribute = attrProxy.AttributeData;
        return true;
    }

    #endregion

    #region Nullable<T>

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

    #endregion

    #region Interface

    public static bool IsImplementsInterface(this ITypeSymbol type, ITypeSymbol interfaceType)
        => type.AllInterfaces.Contains(interfaceType, SymbolEqualityComparer.Default);

    public static bool IsImplementsInterface(this ITypeSymbol type, string interfaceTypeFullName)
    {
        foreach (var itf in type.AllInterfaces) {
            if (itf.MatchDisplayString(interfaceTypeFullName))
                return true;
        }
        return false;
    }

    #endregion

    #region Enumeration

    public static IEnumerable<ISymbol> ContainingSymbols(this ISymbol symbol, bool includeSelf = false)
    {
        if (includeSelf)
            yield return symbol;

        var sym = symbol.ContainingSymbol;
        while (sym is not null) {
            yield return sym;
            sym = sym.ContainingSymbol;
        }
    }

    public static IEnumerable<INamedTypeSymbol> BaseTypes(this INamedTypeSymbol symbol, bool includeSelf = false)
    {
        if (includeSelf)
            yield return symbol;

        var sym = symbol.BaseType;
        while (sym is not null) {
            yield return sym;
            sym = sym.BaseType;
        }
    }

    #endregion

    #region RuntimeHelpers

    public static bool IsRuntimeType(this ITypeSymbol symbol, Type runtimeType, Compilation compilation, bool includeNullability = false)
    {
        var comparer = includeNullability ? SymbolEqualityComparer.IncludeNullability : SymbolEqualityComparer.Default;
        return comparer.Equals(symbol, compilation.GetTypeSymbolByRuntimeType(runtimeType));
    }

    public static bool IsRuntimeType<T>(this ITypeSymbol symbol, Compilation compilation, bool includeNullability = false)
        => symbol.IsRuntimeType(typeof(T), compilation, includeNullability);

    #endregion
}
