namespace Trarizon.Library.Roslyn.SourceInfos.CSharp;

partial class CodeFactory
{
    public static string GeneratedCodeAttributeList(string tool, string version)
        => $"[global::{GeneratedCodeAttributeTypeFullName}(\"{tool}\", \"{version}\")]";

    public static string InterceptsLocationAttributeList(string filePath, int line, int character)
        => $"[global::{InterceptsLocationAttributeTypeFullName}(\"{filePath}\", {line}, {character})]";

    public static string InterceptsLocationAttributeDeclaration() => InterceptsLocationAttributeCSharpDeclaration;

    private const string GeneratedCodeAttributeTypeFullName = "System.CodeDom.Compiler.GeneratedCodeAttribute";
    private const string InterceptsLocationAttributeTypeFullName = "System.Diagnostics.CodeAnalysis.InterceptsLocationAttribute";

    private const string InterceptsLocationAttributeCSharpDeclaration = """
        namespace System.Runtime.CompilerServices
        {
            [global::System.AttributeUsageAttribute(AttributeTargets.Method, AllowMultiple = true)]
            [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
            internal sealed class InterceptsLocationAttribute(string filePath, int line, int character) : global::System.Attribute;
        }
        """;
}
