using Microsoft.CodeAnalysis;

namespace Trarizon.Library.SourceGenerator.Analyzers;
partial class FriendAccessAnalyzer
{
    public const string L_Attribute_TypeName = $"{Literals.CodeAnalysis_MemberAccess_Namespace}.FriendAccessAttribute";
    public const int L_Attribute_FriendTypes_ConstructorIndex = 0;
    public const string L_Attribute_Options_PropertyIdentifier = "Options";

    #region Diagnostics

    public readonly static DiagnosticDescriptor Diagnostic_FriendMemberCannotBeAccessed = new(
        $"TRA{Literals.FriendAccessAnalyzer_Id}0001",
        nameof(Diagnostic_FriendMemberCannotBeAccessed),
        "Cannot access a friend member here",
        Literals.Category,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor Diagnostic_FriendMayBeAccessedByOtherAssembly = new(
        $"TRA{Literals.FriendAccessAnalyzer_Id}0002",
        nameof(Diagnostic_FriendMayBeAccessedByOtherAssembly),
        "Friend member may be accessed by other assembly if not internal",
        Literals.Category,
        DiagnosticSeverity.Warning,
        true);

    public readonly static DiagnosticDescriptor Diagnostic_FriendOnExplicitInterfaceMemberMakeNoSense = new(
        $"TRA{Literals.FriendAccessAnalyzer_Id}0003",
        nameof(Diagnostic_FriendOnExplicitInterfaceMemberMakeNoSense),
        "[FriendAccess] on explicit interface member make no sense",
        Literals.Category,
        DiagnosticSeverity.Warning,
        true);

    public readonly static DiagnosticDescriptor Diagnostic_SpecificTypeInTypeParameterMakeNoSense = new(
        $"TRA{Literals.FriendAccessAnalyzer_Id}0004",
        nameof(Diagnostic_SpecificTypeInTypeParameterMakeNoSense),
        "Specific type in type parameter will be ignored",
        Literals.Category,
        DiagnosticSeverity.Info,
        true);

    #endregion

    private enum FriendAccessOptions
    {
        None = 0,
        AllowInherits = 1,
    }
}
