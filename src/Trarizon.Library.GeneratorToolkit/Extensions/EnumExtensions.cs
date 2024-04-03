using Microsoft.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class EnumExtensions
{
    public static string? DeclarationKeyWord(this TypeKind typeKind) => typeKind switch {
        TypeKind.Class => "class",
        TypeKind.Delegate => "delegate",
        TypeKind.Enum => "enum",
        TypeKind.Interface => "interface",
        TypeKind.Module => "module",
        TypeKind.Struct => "struct",
        TypeKind.Submission => null,
        _ => null,
    };
}
