using Microsoft.CodeAnalysis;
using System.Linq;
using Trarizon.Library.Roslyn.Extensions;

namespace Trarizon.Library.Roslyn;
public static class KnownLiterals
{
    public const string NS_System_CodeDom_Compiler = "System.CodeDom.Compiler";
    public const string NS_System_Runtime_CompilerServices = "System.Runtime.CompilerServices";

    public readonly struct GeneratedCodeAttributeProxy(AttributeData attribute)
    {
        public const string Type_Identifier = "GeneratedCodeAttribute";
        public const string Type_FullName = $"{NS_System_CodeDom_Compiler}.{Type_Identifier}";

        public static string Code_Construct(string tool, string version)
            => $@"global::{Type_FullName}(""{tool}"", ""{version}"")";

        public static bool TryGet(ISymbol symbol, out GeneratedCodeAttributeProxy attribute)
        {
            var attr = symbol.GetAttributes().FirstOrDefault(IsOfThisType);
            attribute = new GeneratedCodeAttributeProxy(attr!);
            return attr is not null;
        }

        public AttributeData AttributeData => attribute;

        public static bool IsOfThisType(AttributeData attribute)
            => attribute.AttributeClass.MatchDisplayString(Type_FullName);

        public string Tool => attribute.GetConstructorArgument(0).Cast<string>()!;

        public string Version => attribute.GetConstructorArgument(1).Cast<string>()!;
    }

    public readonly struct InterceptsLocationAttributeProxy
    {
        public const string Type_Identifier = "InterceptsLocationAttribute";
        public const string Type_FullName = $"{NS_System_Runtime_CompilerServices}.{Type_Identifier}";

        public const string Code_FileScopeDeclaration = """
            namespace System.Runtime.CompilerServices
            {
                [global::System.AttributeUsageAttribute(AttributeTargets.Method, AllowMultiple = true)]
                [global::System.Diagnostics.Conditional("CODE_ANALYSIS")]
                file sealed class InterceptsLocationAttribute(string filePath, int line, int character) : global::System.Attribute;
            }
            """;

        public static string Code_Construct(string filePath, int line, int column)
           => $@"global::{Type_FullName}(@""{filePath}"", {line}, {column})";
    }
}
