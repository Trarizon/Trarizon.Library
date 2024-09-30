#if false

namespace Trarizon.Library.SourceGeneration.Interceptors;
internal static class EnumHasAnyFlagLiterals
{
    public const string L_HasAnyFlag_MethodIdentifier = "HasAnyFlag";
    public const string L_TraEnum_TypeName = $"{Literals.Library_Namespace}.TraEnum";
    public const string L_Interceptors_TypeName = "__EnumHasAnyFlagInterceptors";
    public const string L_FileName = "EnumHasAnyFlagInterceptor.g.cs";

    public static string L_InterceptorMethodDeclaration(string type)
        => $"public static bool HasAnyFlag(this {type} value, {type} flag) => (value & flag) != 0;";
}

#endif