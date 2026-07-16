using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Roslyn;

public static partial class SymbolExtensions
{
    #region Properties

    private static SymbolDisplayFormat? _defaultWithGenericSymbolDisplayFormat;
    private static SymbolDisplayFormat? _fullyQualifiedWithNullableAnnotationFormat;

    extension(SymbolDisplayFormat)
    {
        /// <remarks>
        /// eg: <c>List&lt;T></c> as <c>System.Collections.Generic.List</c>
        /// </remarks>
        public static SymbolDisplayFormat DefaultWithoutGenerics => _defaultWithGenericSymbolDisplayFormat ??=
             SymbolDisplayFormat.CSharpErrorMessageFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None);

        public static SymbolDisplayFormat FullyQualifiedWithNullableAnnotation => _fullyQualifiedWithNullableAnnotationFormat ??=
            SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
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

    public static ImmutableArray<AttributeData> GetAttributeDatas(this ISymbol symbol, INamedTypeSymbol attributeType)
    {
        var builder = ImmutableArray.CreateBuilder<AttributeData>();
        foreach (var attr in symbol.GetAttributes()) {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeType)) {
                builder.Add(attr);
            }
        }
        return builder.ToImmutableArray();
    }

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

    public static ImmutableArray<AttributeData> GetAttributeDatasByFullyQualifiedMetadataName(this ISymbol symbol, string fullyQualifiedMetadataName)
    {
        var builder = ImmutableArray.CreateBuilder<AttributeData>();
        foreach (var attr in symbol.GetAttributes()) {
            if (attr.AttributeClass?.MatchMetadataName(fullyQualifiedMetadataName) is true) {
                builder.Add(attr);
            }
        }
        return builder.ToImmutableArray();
    }

    public static bool TryGetAttributeDataByFullyQualifiedMetadataName(this ISymbol symbol, string fullyQualifiedMetadataName, [MaybeNullWhen(false)] out AttributeData attributeData)
    {
        foreach (var attr in symbol.GetAttributes()) {
            if (attr.AttributeClass?.MatchMetadataName(fullyQualifiedMetadataName) == true) {
                attributeData = attr;
                return true;
            }
        }
        attributeData = null;
        return false;
    }

    public static bool HasAttributeByFullyQualifiedMetadataName(this ISymbol symbol, string fullyQualifiedMetadataName)
        => symbol.TryGetAttributeDataByFullyQualifiedMetadataName(fullyQualifiedMetadataName, out _);

    #endregion

    #region Type

    public static bool IsImplements(this ITypeSymbol type, ITypeSymbol interfaceType)
    {
        if (interfaceType.TypeKind is not TypeKind.Interface)
            return false;

        foreach (var it in type.AllInterfaces) {
            if (SymbolEqualityComparer.Default.Equals(it, interfaceType))
                return true;
        }
        return false;
    }

    public static bool IsImplementsByFullyQualifiedMetadataName(this ITypeSymbol type, string fullyQualifiedMetadataName)
    {
        foreach (var it in type.AllInterfaces) {
            if (it.MatchMetadataName(fullyQualifiedMetadataName))
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

    public static bool IsInheritsByFullyQualifiedMetadataName(this ITypeSymbol type, string fullyQualifiedMetadataName)
    {
        var sym = type.BaseType;
        while (sym is not null) {
            if (sym.MatchMetadataName(fullyQualifiedMetadataName) == true)
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

    #region Name

    public static bool MatchMetadataName(this ITypeSymbol symbol, string fullyQualifiedMetadataName)
        => MatchMetadataName(symbol, fullyQualifiedMetadataName.AsSpan());

    public static bool MatchMetadataName(this ITypeSymbol symbol, ReadOnlySpan<char> fullyQualifiedMetadataName)
    {
        return Core(symbol, fullyQualifiedMetadataName);

        static bool Core(ISymbol symbol, ReadOnlySpan<char> fullyQualifiedMetadataName)
        {
            string name;
            switch (symbol) {
                // Nested namespace
                case INamedTypeSymbol { ContainingNamespace.IsGlobalNamespace: false }:
                    name = symbol.MetadataName;
                    return CompareWithPrefix(name, fullyQualifiedMetadataName, '.')
                        && Core(symbol.ContainingNamespace, fullyQualifiedMetadataName[..^name.Length]);

                // Top namespace
                case INamespaceSymbol { IsGlobalNamespace: false }:
                    name = symbol.MetadataName;
                    return name.SequenceEqual(fullyQualifiedMetadataName);

                // Top type
                case ITypeSymbol { ContainingSymbol: INamespaceSymbol { IsGlobalNamespace: true } }:
                    name = symbol.MetadataName;
                    return name.SequenceEqual(fullyQualifiedMetadataName);

                // Top type in namespace
                case ITypeSymbol { ContainingType: null }:
                    name = symbol.MetadataName;
                    return CompareWithPrefix(name, fullyQualifiedMetadataName, '.')
                        && Core(symbol.ContainingNamespace, fullyQualifiedMetadataName[..^name.Length]);

                // Nested type
                case ITypeSymbol { ContainingType: { } }:
                    name = symbol.MetadataName;
                    return CompareWithPrefix(name, fullyQualifiedMetadataName, '+')
                        && Core(symbol.ContainingType, fullyQualifiedMetadataName[..^name.Length]);
            }
            Debug.Fail("Unknown symbol type");
            return false;

            static bool CompareWithPrefix(string metadataName, ReadOnlySpan<char> fullyQualifiedMetadataName, char prefix)
                => fullyQualifiedMetadataName.Length > metadataName.Length
                && fullyQualifiedMetadataName[^metadataName.Length] == prefix
                && metadataName.SequenceEqual(fullyQualifiedMetadataName[^metadataName.Length..]);
        }

    }

    #endregion
}
