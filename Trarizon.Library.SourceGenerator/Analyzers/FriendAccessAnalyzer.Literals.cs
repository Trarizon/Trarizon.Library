using Microsoft.CodeAnalysis;

namespace Trarizon.Library.SourceGenerator.Analyzers;
partial class FriendAccessAnalyzer
{
    private static class Literals
    {
        public const string Attribute_TypeName = $"{Constants.Namespace_CodeAnalysis}.MemberAccess.FriendAccessAttribute";
        public const int Attribute_FriendTypes_ConstructorIndex = 0;

        #region Diagnostics

        public readonly static DiagnosticDescriptor Diagnostic_FriendMemberCannotBeAccessed = new(
            $"TRA{Constants.FriendAccessAnalyzer_Id}0001",
            nameof(Diagnostic_FriendMemberCannotBeAccessed),
            "Cannot access a friend member here",
            Constants.Category,
            DiagnosticSeverity.Error,
            true);

        public readonly static DiagnosticDescriptor Diagnostic_FriendMayBeAccessedByOtherAssembly = new(
            $"TRA{Constants.FriendAccessAnalyzer_Id}0002",
            nameof(Diagnostic_FriendMayBeAccessedByOtherAssembly),
            "Friend member may be accessed by other assembly if not internal",
            Constants.Category,
            DiagnosticSeverity.Warning,
            true);

        public readonly static DiagnosticDescriptor Diagnostic_FriendOnExplicitInterfaceMemberMakeNoSense = new(
            $"TRA{Constants.FriendAccessAnalyzer_Id}0003",
            nameof(Diagnostic_FriendOnExplicitInterfaceMemberMakeNoSense),
            "[FriendAccess] on explicit interface member make no sense",
            Constants.Category,
            DiagnosticSeverity.Warning,
            true);

        #endregion
    }
}
