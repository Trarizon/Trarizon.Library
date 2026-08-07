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
    private TypeUnionData? Parse(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context is not
            {
                TargetNode: StructDeclarationSyntax syntax,
                TargetSymbol: INamedTypeSymbol symbol,
                Attributes: [var attr]
            })
            return null;

        var variantTypes = attr.GetConstructorArgument(0).CastArray<ITypeSymbol>();
        return ParseCore(syntax, symbol, attr, variantTypes, cancellationToken);
    }

    private TypeUnionData? ParseGeneric(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context is not
            {
                TargetNode: StructDeclarationSyntax syntax,
                TargetSymbol: INamedTypeSymbol symbol,
                Attributes: [var attr]
            })
            return null;

        if (attr.AttributeClass is null)
            return null;

        var variantTypes = attr.AttributeClass.TypeArguments;
        return ParseCore(syntax, symbol, attr, variantTypes, cancellationToken);
    }

    private TypeUnionData? ParseCore(StructDeclarationSyntax syntax, INamedTypeSymbol symbol, AttributeData attr, ImmutableArray<ITypeSymbol> variantTypes, CancellationToken cancellationToken)
    {
        if (variantTypes.Length == 0)
            return null;

        int unmanagedIdx = 0;
        var unmanagedMap = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);
        int managedIdx = 0;
        var managedMap = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);

        var readableNameMap = new Dictionary<string, int>();

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

                vtk = type switch
                {
                    { SpecialType: SpecialType.System_Void } => VariantTypeKind.Void,
                    IPointerTypeSymbol { PointedAtType.SpecialType: SpecialType.System_Void } => VariantTypeKind.VoidPointer,
                    IPointerTypeSymbol => VariantTypeKind.Pointer,
                    { TypeKind: TypeKind.FunctionPointer } => VariantTypeKind.FunctionPointer,
                    _ => VariantTypeKind.Unmanaged,
                };
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
                id, fqname, vtk, type.IsRefLikeType, isInterface, fieldId, GetUniqueReadableName(type, readableNameMap)
            );
            variantDatas.Add(data);

            string GetDefaultReadableName(ITypeSymbol type)
            {
                var str = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                var res = (stackalloc char[str.Length]);

                var idx = 0;
                foreach (var c in str.AsSpan())
                {
                    // skip consecutive underscores
                    if (idx > 0 && res[idx - 1] == '_' && c == '_')
                        continue;
                    if (c is '_' or (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9'))
                        res[idx++] = c;
                    else
                        res[idx++] = '_';
                }

                // remove trailing underscores
                while (idx > 0 && res[idx - 1] == '_')
                    idx--;

                return res[..idx].ToString();
            }

            string GetUniqueReadableName(ITypeSymbol type, Dictionary<string, int> map)
            {
                var name = GetDefaultReadableName(type);

                if (!map.TryGetValue(name, out var idx))
                {
                    map.Add(name, 0);
                    return name;
                }

            Inc:
                idx++;
                var resultName = $"{name}_{idx}";
                if (map.ContainsKey(resultName))
                {
                    goto Inc;
                }

                map.Add(resultName, 0);
                return resultName;
            }
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
