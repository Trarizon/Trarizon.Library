using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using Trarizon.Library.Linq;
using Trarizon.Library.Roslyn;
using Trarizon.Library.Roslyn.CSharp;
using Trarizon.Library.Roslyn.Pipeline;
using Trarizon.Library.Roslyn.Pipeline.Collections;

namespace Trarizon.Library.Functional.Generators.TypeUnion;

partial class TypeUnionGenerator
{
    private Optional<TypeUnionData> Parse(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context is not
            {
                TargetNode: StructDeclarationSyntax syntax,
                TargetSymbol: INamedTypeSymbol symbol,
                Attributes: [var attr]
            })
            return default;

        var variantTypes = attr.GetConstructorArgument(0).CastArray<ITypeSymbol>();
        return ParseCore(syntax, symbol, attr, variantTypes, cancellationToken);
    }

    private Optional<TypeUnionData> ParseGeneric(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context is not
            {
                TargetNode: StructDeclarationSyntax syntax,
                TargetSymbol: INamedTypeSymbol symbol,
                Attributes: [var attr]
            })
            return default;

        if (attr.AttributeClass is null)
            return default;

        var variantTypes = attr.AttributeClass.TypeArguments;
        return ParseCore(syntax, symbol, attr, variantTypes, cancellationToken);
    }

    private Optional<TypeUnionData> ParseCore(StructDeclarationSyntax syntax, INamedTypeSymbol symbol, AttributeData attr, ImmutableArray<ITypeSymbol> variantTypes, CancellationToken cancellationToken)
    {
        if (variantTypes.Length == 0)
            return default;

        int unmanagedIdx = 0;
        var unmanagedMap = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);
        int managedIdx = 0;
        var managedMap = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);

        List<VariantData> variantDatas = new();
        foreach (var (index, type) in variantTypes.Index())
        {
            var id = index + 1;
            var fqname = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            VariantTypeKind vtk;
            bool isInterface;
            int fieldId;
            if (type.IsReferenceType)
            {
                vtk = VariantTypeKind.Reference;
                isInterface = type.TypeKind is TypeKind.Interface;
                fieldId = default;
            }
            else if (type.IsUnmanagedType)
            {
                if (!unmanagedMap.TryGetValue(type, out var idx))
                {
                    idx = unmanagedIdx++;
                    unmanagedMap.Add(type, idx);
                }

                vtk = type.TypeKind is TypeKind.Pointer ? VariantTypeKind.Pointer : VariantTypeKind.Unmanaged;
                isInterface = false;
                fieldId = idx;
            }
            else
            {
                if (!managedMap.TryGetValue(type, out var idx))
                {
                    idx = managedIdx++;
                    managedMap.Add(type, idx);
                }
                vtk = VariantTypeKind.Managed;
                isInterface = false;
                fieldId = idx;
            }

            var data = new VariantData(
                id, fqname, vtk, type.IsRefLikeType, isInterface, fieldId
            );
            variantDatas.Add(data);
        }

        var shareInterfaceOption = attr.GetNamedArgument("ShareInterface").CastValueOrDefault<UnionShareInterfaceOption>();

        // var sharedInterfaces = ParseSharedInterfaces(variantTypes);

        return new TypeUnionData(
            CodeHelpers.ToFileNameString(symbol.ToDisplayString()),
            TypeHierarchyInfo.Create(symbol, syntax),
            variantDatas.ToSequenceEquatableImmutableArray()
        );
    }

    private SequenceEquatableImmutableArray<VariantInterfaceData> ParseSharedInterfaces(ImmutableArray<ITypeSymbol> variantTypes)
    {
        var sharedInterfaces = variantTypes
            .Select(x =>
            {
                if (x.TypeKind is TypeKind.Interface)
                    return x.AllInterfaces.AsEnumerable().Prepend(x);
                else
                    return x.AllInterfaces.AsEnumerable();
            })
            .Aggregate((l, r) => l.Intersect(r, (IEqualityComparer<ITypeSymbol>)SymbolEqualityComparer.Default))
            // sharedInterfaces
            .Select(x =>
            {
                var fqname = x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                return new VariantInterfaceData(
                    fqname,
                    x.GetMembers()
                        .Where(x =>
                        {
                            if (x.IsImplicitlyDeclared)
                                return false;
                            if (x is IMethodSymbol m)
                                return m.MethodKind is MethodKind.Ordinary;
                            return true;
                        })
                        .Select(CollectInterfaceMemberData)
                        .ToSequenceEquatableImmutableArray()
                );
            })
            .ToSequenceEquatableImmutableArray();

        return sharedInterfaces;

        VariantInterfaceMemberData CollectInterfaceMemberData(ISymbol symbol)
        {
            if (symbol is IPropertySymbol prop)
            {
                return new VariantInterfaceMemberData(

                );
            }

            return default;
        }
    }
}
