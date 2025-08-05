using System.Reflection;
using Trarizon.Library.Roslyn.SourceInfos.CSharp;

namespace Trarizon.Library.CodeAnalysis.SourceGeneration.Literals;
internal static class Codes
{
    private static readonly string GeneratedCodeAttributeTool = Assembly.GetExecutingAssembly().GetName().Name;
    private static readonly string GeneratedCodeAttributeVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    public static readonly string GeneratedCodeAttributeList = CodeFactory.GeneratedCodeAttributeList(GeneratedCodeAttributeTool, GeneratedCodeAttributeVersion);
}

internal static class DiagnosticIds
{
    public const string FriendAccess = "TRACA00";
    public const string Singleton = "TRACA01";
    public const string ExternalSealed = "TRACA02";
}

public static class Categories
{
    public const string Default = "Trarizon.Library.CodeAnalysis";
    public const string AccessControl = Default + ".AccessControl";
}

public static class Namespaces
{
    public const string TrarizonCodeAnalysis = "Trarizon.Library.CodeAnalysis";
    public const string TrarizonDiagnostics = "Trarizon.Library.CodeAnalysis.Diagnostics";
    public const string TrarizonGeneration = "Trarizon.Library.CodeAnalysis.Generation";
}
