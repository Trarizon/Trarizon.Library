using Microsoft.CodeAnalysis;

namespace Trarizon.Library.SourceGenerator.Analyzers;
partial class FriendAnalyzer
{
    private static class Literals
    {
        public const string FriendAttribute_TypeName = "Trarizon.Library.CodeAnalysis.FriendAttribute";
        public const int FriendAttribute_FriendTypes_ConstructorIndex = 0;

        public readonly static DiagnosticDescriptor Diagnostic_FriendMemberCannotBeAccessed = new(
            $"TRA{DiagnosticIds.FriendAnalyzer}0001",
            nameof(Diagnostic_FriendMemberCannotBeAccessed),
            "Cannot access a friend member here",
            "Trarizon.Library.SourceGenerator",
            DiagnosticSeverity.Error,
            true);

        public readonly static DiagnosticDescriptor Diagnostic_FriendMayBeAccessedByOtherAssembly = new(
            $"TRA{DiagnosticIds.FriendAnalyzer}0002",
            nameof(Diagnostic_FriendMayBeAccessedByOtherAssembly),
            "Friend member may be accessed by other assembly if not internal",
            "Trarizon.Library.SourceGenerator",
            DiagnosticSeverity.Warning,
            true);
    }
}
