namespace Trarizon.Library.SourceGenerator;
internal static class Constants
{
    public const string Category = "Trarizon.Library.SourceGenerator";
    public const string Namespace_SourceGeneraterGenerators = $"{Category}.Generators";

    public const string Namespace_CodeAnalysis = "Trarizon.Library.CodeAnalysis";
    public const string Namespace_CodeTemplating = "Trarizon.Library.CodeTemplating";

    public const string Version = "0.3.0";

    #region DiagnosticIds

    public const string FriendAccessAnalyzer_Id = "FA";
    public const string BackingFieldAccessAnalyzer_Id = "BFA";

    public const string SingletonGenerator_Id = "Siglt";
    public const string SingletonGenerator_Suffix = "Singleton";

    #endregion
}
