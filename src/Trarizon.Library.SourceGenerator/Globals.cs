namespace Trarizon.Library.SourceGenerator;
internal static class Globals
{
    public const string System_Namespace = "System";
    public const string System_Diagnostics_CodeAnalysis_Namespcae = "System.Diagnostics.CodeAnalysis";
    public const string System_Runtime_InteropServices_Namespace = "System.Runtime.InteropServices";
    public const string System_Runtime_CompilerServices_Namespace = "System.Runtime.CompilerServices";

    public const string Unsafe_TypeName = $"{System_Runtime_CompilerServices_Namespace}.Unsafe";
    public const string StructLayoutAttribute_TypeName = $"{System_Runtime_InteropServices_Namespace}.StructLayoutAttribute";
    public const string MaybeNullWhenAttribute_TypeName = $"{System_Diagnostics_CodeAnalysis_Namespcae}.MaybeNullWhenAttribute";

    public const string LayoutKind_TypeName = $"{System_Runtime_InteropServices_Namespace}.LayoutKind";
    public const string LayoutKind_Explicit_EnumValue = $"{LayoutKind_TypeName}.Explicit";

    public const string As_Identifier = $"As";
    public const string Unbox_Identifier = $"Unbox";

    public const string FieldOffsetAttribute_TypeName = $"{System_Runtime_InteropServices_Namespace}.FieldOffsetAttribute";
}
