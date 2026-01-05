using System.Reflection;
using Trarizon.Library.Roslyn.SourceInfos.CSharp;

namespace Trarizon.Library.CodeAnalysis.SourceGeneration.Internal;

internal static class Codes
{
    public const string NamespaceGenerated = "Trarizon.Library.CodeAnalysis.Generated";

    private static readonly string GeneratedCodeAttributeTool = Assembly.GetExecutingAssembly().GetName().Name;
    private static readonly string GeneratedCodeAttributeVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    public static readonly string GeneratedCodeAttributeList = CodeFactory.GeneratedCodeAttributeList(GeneratedCodeAttributeTool, GeneratedCodeAttributeVersion);
}

internal static class DiagnosticIds
{
    public const string FriendAccess = "TRACA00";
    public const string Singleton = "TRACA01";
    public const string ExternalSealed = "TRACA02";
    public const string TSelf = "TRACA03";
}

public static class Categories
{
    public const string Default = "Trarizon.Library.CodeAnalysis";
    public const string AccessControl = Default + ".AccessControl";
}

