using System.Diagnostics;

namespace Trarizon.Library.Functional.Generators.TypeUnion;

sealed record VariantData(
    int Id,
    string TypeFQName,
    VariantTypeKind TypeKind,
    bool IsRefLikeType,
    bool IsInterface,
    int FieldId
)
{
    public string FieldTypeFQName
    {
        get
        {
            if (TypeKind is VariantTypeKind.Pointer)
            {
                Debug.Assert(TypeFQName.EndsWith("*"));
                if (TypeFQName == "void*")
                    return TypeUnionGenerator.VoidPointerHelperFQName;
                return $"{TypeUnionGenerator.PointerHelperFQName}<{TypeFQName[..^1]}>";
            }
            return TypeFQName;
        }
    }
}

enum VariantTypeKind { Managed, Reference, Unmanaged, Pointer, Void, }

static partial class DataExtensions
{
    public static bool IsUnmanaged(this VariantTypeKind kind)
        => kind is VariantTypeKind.Unmanaged or VariantTypeKind.Pointer;
}