namespace Trarizon.Library.GeneratorToolkit;
internal static class Literals
{
    public const string System_CodeDom_Compiler_Namespace = "System.CodeDom.Compiler";
    public const string System_Runtime_CompilerServices = "System.Runtime.CompilerServices";

    public const string GeneratedCodeAttribute_TypeName = $"{System_CodeDom_Compiler_Namespace}.{GeneratedCodeAttribute_TypeIdentifier}";
    public const string InterceptsLocationAttribute_TypeName = $"{System_Runtime_CompilerServices}.{InterceptsLocationAttribute_TypeIdentifier}";

    public const string GeneratedCodeAttribute_TypeIdentifier = "GeneratedCodeAttribute";
    public const int GeneratedCodeAttribute_Tool_ConstructorIndex = 0;
    public const int GeneratedCodeAttribute_Version_ConstructorIndex = 1;

    public const string InterceptsLocationAttribute_TypeIdentifier = "InterceptsLocationAttribute";

    // INamespaceSymbol has prop: IsGlobalNamespace
    // But this remained as a note
    // public const string GlobalNamespaceDisplayString = "<global namespace>";
}
