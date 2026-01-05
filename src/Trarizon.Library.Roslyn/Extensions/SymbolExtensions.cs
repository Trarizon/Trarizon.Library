using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.Library.Linq;

namespace Trarizon.Library.Roslyn.Extensions;
public static partial class SymbolExtensions
{
    #region Properties

    private static SymbolDisplayFormat? _defaultWithGenericSymbolDisplayFormat;

    extension(SymbolDisplayFormat)
    {
        /// <remarks>
        /// eg: <c>List&lt;T></c> as <c>System.Collections.Generic.List</c>
        /// </remarks>
        public static SymbolDisplayFormat DefaultWithoutGenerics => _defaultWithGenericSymbolDisplayFormat ??=
             SymbolDisplayFormat.CSharpErrorMessageFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None);
    }

    #endregion

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

    #region Attribute

    public static bool TryGetAttributeData(this ISymbol symbol, INamedTypeSymbol attributeType, [MaybeNullWhen(false)] out AttributeData attributeData)
    {
        foreach (var attr in symbol.GetAttributes()) {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeType)) {
                attributeData = attr;
                return true;
            }
        }
        attributeData = null;
        return false;
    }

    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attributeType)
        => symbol.TryGetAttributeData(attributeType, out _);

    public static bool TryGetAttributeData(this ISymbol symbol, string attributeTypeFullName, [NotNullWhen(true)] out AttributeData? attributeData)
    {
        foreach (var attr in symbol.GetAttributes()) {
            if (attr.AttributeClass?.ToDisplayString() == attributeTypeFullName) {
                attributeData = attr;
                return true;
            }
        }
        attributeData = null;
        return false;
    }

    public static bool HasAttribute(this ISymbol symbol, string attributeTypeFullName)
        => symbol.TryGetAttributeData(attributeTypeFullName, out _);

    #endregion

    #region Type

    public static bool IsImplements(this ITypeSymbol type, ITypeSymbol interfaceType)
    {
        if (interfaceType.TypeKind is not TypeKind.Interface)
            return false;

        return type.AllInterfaces.Contains(interfaceType, SymbolEqualityComparer.Default);
    }

    public static bool IsInherits(this ITypeSymbol type, ITypeSymbol baseType)
    {
        if (baseType.TypeKind is not TypeKind.Class)
            return false;

        var sym = type.BaseType;
        while (sym is not null) {
            if (SymbolEqualityComparer.Default.Equals(sym, baseType))
                return true;
            sym = sym.BaseType;
        }
        return false;
    }

    public static IEnumerable<INamedTypeSymbol> BaseTypes(this INamedTypeSymbol type, bool includeSelf = false)
    {
        if (includeSelf)
            yield return type;

        var sym = type.BaseType;
        while (sym is not null) {
            yield return sym;
            sym = sym.BaseType;
        }
    }

    public static bool IsNullableValueType(this ITypeSymbol type, [NotNullWhen(true)] out ITypeSymbol? underlyingType)
    {
        if (type is { IsValueType: true, NullableAnnotation: NullableAnnotation.Annotated }) {
            underlyingType = ((INamedTypeSymbol)type).TypeArguments[0];
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
}
