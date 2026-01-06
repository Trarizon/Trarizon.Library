using Trarizon.Library.Roslyn.SourceInfos.CSharp;

namespace Trarizon.Library.Functional.SourceGeneration;

internal static class Codes
{
    public static readonly string GeneratedCodeAttributeList = CodeFactory.GeneratedCodeAttributeList(
        typeof(Codes).Assembly.GetName().Name,
        typeof(Codes).Assembly.GetName().Version.ToString());
}
