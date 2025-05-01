using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using Trarizon.Library.Roslyn.Extensions;

namespace Trarizon.Library.Generators.CodeAnalysis;
partial class FriendAccessAnalyzer
{
    private readonly struct AttributeProxy(AttributeData attribute)
    {
        private const string Attribute_FullTypeName = $"{Literals.NS_CodeAnalysis}.FriendAccessAttribute";
        
        public static bool IsThisType(AttributeData attributeData)
            => attributeData.AttributeClass.MatchDisplayString(Attribute_FullTypeName);

        public ImmutableArray<ITypeSymbol> FriendTypes => attribute
            .GetConstructorArgument(0)
            .Value
            .CastArray<ITypeSymbol>();

        public FriendAccessOptionsMirror Options => attribute
            .GetNamedArgument("Options")
            .Value
            .Cast<FriendAccessOptionsMirror>();
    }

    [Flags]
    private enum FriendAccessOptionsMirror
    {
        None = 0,
        /// <summary>
        /// Allow allow derived types of given type to access
        /// </summary>
        AllowInherits = 1,
    }

    private static class Diag
    {
        public readonly static DiagnosticDescriptor FriendMemberCannotBeAccessed = new(
            $"TRA{Literals.FriendAccessAnalyzer_Id}0001",
            nameof(FriendMemberCannotBeAccessed),
            "Cannot access a friend member here",
            Literals.Category,
            DiagnosticSeverity.Error,
            true);

        public readonly static DiagnosticDescriptor FriendMayBeAccessedByOtherAssembly = new(
            $"TRA{Literals.FriendAccessAnalyzer_Id}0002",
            nameof(FriendMayBeAccessedByOtherAssembly),
            "Friend member may be accessed by other assembly if not internal",
            Literals.Category,
            DiagnosticSeverity.Error,
            true);

        public readonly static DiagnosticDescriptor FriendOnExplicitInterfaceMemberMakeNoSense = new(
            $"TRA{Literals.FriendAccessAnalyzer_Id}0003",
            nameof(FriendOnExplicitInterfaceMemberMakeNoSense),
            "[FriendAccess] on explicit interface member make no sense",
            Literals.Category,
            DiagnosticSeverity.Warning,
            true);

        public readonly static DiagnosticDescriptor SpecificTypeInTypeParameterMakeNoSense = new(
            $"TRA{Literals.FriendAccessAnalyzer_Id}0004",
            nameof(SpecificTypeInTypeParameterMakeNoSense),
            "Specific type in type parameter will be ignored",
            Literals.Category,
            DiagnosticSeverity.Info,
            true);
    }
}
