namespace Trarizon.Library.GeneratorToolkit;
internal static class Literals
{
    public const string System_CodeDom_Compiler_Namespace = "System.CodeDom.Compiler";

    public const string GeneratedCodeAttribute_TypeName = $"{System_CodeDom_Compiler_Namespace}.{GeneratedCodeAttribute_TypeIdentifier}";

    public const string GeneratedCodeAttribute_TypeIdentifier = "GeneratedCodeAttribute";
    public const int GeneratedCodeAttribute_Tool_ConstructorIndex = 0;
    public const int GeneratedCodeAttribute_Version_ConstructorIndex = 1;

    // INamespaceSymbol has prop: IsGlobalNamespace
    // But this remained as a note
    // public const string GlobalNamespaceDisplayString = "<global namespace>";
}
