using Microsoft.CodeAnalysis;
using CsTypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Trarizon.Library.Functional.Generators.TypeUnion;

sealed record VariantData(
    int Id,
    VariantTypeData TypeData,
    int FieldId,
    // Name that can use as identifier in code, MinimalQualifiedFormat maybe with number suffix
    // for pointer type, it is the original type name,
    string ReadableIdentifier
);

record VariantTypeData(
    string FullyQName,
    string MinimalQName,
    VariantTypeKind TypeKind,
    bool IsRefLikeType,
    bool IsInterface,
    // Pointer type has a subtype data
    VariantTypeData? SubtypeData
)
{
    public int PointerLevel => TypeKind is VariantTypeKind.Pointer ? 1 + SubtypeData!.PointerLevel : 0;
    public bool IsNonVoidPointer => TypeKind is VariantTypeKind.Pointer && !(SubtypeData!.TypeKind is VariantTypeKind.Void || SubtypeData.IsVoidPointer);
    public bool IsVoidPointer => TypeKind is VariantTypeKind.Pointer && (SubtypeData!.TypeKind is VariantTypeKind.Void || SubtypeData.IsVoidPointer);
    public VariantTypeData FinalPointerAtType => TypeKind is VariantTypeKind.Pointer ? SubtypeData!.FinalPointerAtType : this;

    public static VariantTypeData Create(ITypeSymbol type)
    {
        var fqname = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var mqname = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        VariantTypeKind vtk;
        bool isInterface;
        VariantTypeData? sub = null;
        if (type.IsReferenceType)
        {
            vtk = VariantTypeKind.Reference;
            isInterface = type.TypeKind is CsTypeKind.Interface;
        }
        else if (type.IsUnmanagedType)
        {
            if (type.TypeKind is CsTypeKind.Pointer)
            {
                sub = Create(((IPointerTypeSymbol)type).PointedAtType);
                vtk = VariantTypeKind.Pointer;
            }
            else
            {
                vtk = type switch
                {
                    { SpecialType: SpecialType.System_Void } => VariantTypeKind.Void,
                    { TypeKind: CsTypeKind.FunctionPointer } => VariantTypeKind.FunctionPointer,
                    _ => VariantTypeKind.Unmanaged,
                };
            }
            isInterface = false;
        }
        else
        {
            vtk = VariantTypeKind.Managed;
            isInterface = false;
        }
        return new(fqname, mqname, vtk, type.IsRefLikeType, isInterface, sub);
    }
}

enum VariantTypeKind { Managed, Reference, Unmanaged, Pointer, FunctionPointer, Void, }

static partial class DataExtensions
{
    extension(VariantTypeKind kind)
    {
        public bool IsUnmanaged => kind is VariantTypeKind.Unmanaged || kind.IsPointer;
        public bool IsGenericable => kind is not VariantTypeKind.Void && !kind.IsPointer;
        public bool IsPointer => kind is VariantTypeKind.Pointer or VariantTypeKind.FunctionPointer;
    }
}