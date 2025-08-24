namespace Trarizon.Library.Roslyn.SourceInfos;
public static class KnownInfos
{
    public static class GeneratedCodeAttribute
    {
        public const string TypeFullName = "System.CodeDom.Compiler.GeneratedCodeAttribute";
    }

    internal static class InterceptsLocationAttribute
    {
        public const string TypeFullName = "System.Runtime.CompilerServices.InterceptsLocationAttribute";

        public const string CSharpFullDeclaration = """
            namespace System.Runtime.CompilerServices
            {
                [global::System.AttributeUsageAttribute(AttributeTargets.Method, AllowMultiple = true)]
                [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
                internal sealed class InterceptsLocationAttribute(string filePath, int line, int character) : global::System.Attribute;
            }
            """;
    }
}
