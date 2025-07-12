using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.Library.Collections;

namespace Trarizon.Library.Roslyn.Extensions;
public static partial class SymbolExtensions
{
    #region Inheritance

    public static bool IsImplements(this ITypeSymbol type, ITypeSymbol interfaceType)
    {
        if (interfaceType.TypeKind is not TypeKind.Interface)
            return false;

        return type.AllInterfaces.Contains(interfaceType, SymbolEqualityComparer.Default);
    }

    public static bool IsImplements(this ITypeSymbol type, string interfaceTypeFullName)
    {
        foreach (var itf in type.AllInterfaces) {
            if (itf.MatchDisplayString(interfaceTypeFullName))
                return true;
        }
        return false;
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

    public static bool IsInherits(this ITypeSymbol type,string baseTypeFullName)
    {
        var sym = type.BaseType;
        while (sym is not null) {
            if (sym.MatchDisplayString(baseTypeFullName))
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

    #endregion

    #region Nullable

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
