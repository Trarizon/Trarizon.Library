using Microsoft.CodeAnalysis;
using Trarizon.Library.CodeAnalysis.SourceGeneration.Literals;

namespace Trarizon.Library.CodeAnalysis.SourceGeneration;
partial class FriendAccessAnalyzer
{
    static class Descriptors
    {
        public static readonly DiagnosticDescriptor FriendMemberCannotBeAccessed = new(
            DiagnosticIds.FriendAccess + "01",
            "Friend member cannot be accessable",
            "Only friend type can access this member",
            Categories.AccessControl,
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor FriendMemberMayBeAccessedByOtherAssembly = new(
            DiagnosticIds.FriendAccess + "02",
            "Friend member is public accessable",
            "Friend member may be accessed by other assembly",
            Categories.AccessControl,
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor FriendTypeShouldNotOnExplicitInterfaceMember = new(
            DiagnosticIds.FriendAccess + "03",
            "Friend type should not be on explicit interface member",
            "[FriendAccess] marked on explicit interface member will be ignored",
            Categories.AccessControl,
            DiagnosticSeverity.Warning,
            true);

        public static readonly DiagnosticDescriptor FriendTypeRecommendBeUnbounded = new(
            DiagnosticIds.FriendAccess + "04",
            "Type argument will be ignored",
            "Friend type's type argument will be ignored, we recommend you use unbounded generic type instead",
            Categories.AccessControl,
            DiagnosticSeverity.Info,
            true);
    }
}
