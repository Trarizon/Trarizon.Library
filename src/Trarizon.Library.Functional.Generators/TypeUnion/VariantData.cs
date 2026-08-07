using System.Diagnostics;

namespace Trarizon.Library.Functional.Generators.TypeUnion;

sealed record VariantData(
    int Id,
    string TypeFQName,
    VariantTypeKind TypeKind,
    bool IsRefLikeType,
    bool IsInterface,
    int FieldId,
    // Name that can use as identifier in code, MinimalQualifiedFormat maybe with number suffix
    // for pointer type, it is the original type name
    string ReadableName
);

enum VariantTypeKind { Managed, Reference, Unmanaged, Pointer, VoidPointer, FunctionPointer, Void, }

static partial class DataExtensions
{
    public static bool IsUnmanaged(this VariantTypeKind kind)
        => kind is VariantTypeKind.Unmanaged || kind.IsPointer();

    public static bool IsGenericable(this VariantTypeKind kind)
        => kind is not VariantTypeKind.Void && !kind.IsPointer();

    public static bool IsPointer(this VariantTypeKind kind)
        => kind is VariantTypeKind.Pointer or VariantTypeKind.VoidPointer or VariantTypeKind.FunctionPointer;
}