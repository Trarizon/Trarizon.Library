using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.Library.Roslyn.Extensions;

namespace Trarizon.Library.Generators;
partial class BackingFieldAccessAnalyzer
{
    private readonly struct AttributeProxy(AttributeData attribute)
    {
        public static bool TryGet(ISymbol symbol, out AttributeProxy attribute)
        {
            var attr = symbol.GetAttributes().FirstOrDefault(IsThisType);
            attribute = new(attr!);
            return attr is not null;
        }

        public static bool IsThisType(AttributeData attribute)
            => attribute.AttributeClass.MatchDisplayString($"{Literals.NS_CodeAnalysis}.BackingFieldAccessAttribute");

        public ImmutableArray<string> AccessableMembers => attribute
            .GetConstructorArgument(0)
            .CastArray<string>();
    }

    private static class Diag
    {
        public static readonly DiagnosticDescriptor BackingFieldShouldBePrivate = new(
            $"TRA{Literals.BackingFieldAccessAnalyzer_Id}01",
            nameof(BackingFieldShouldBePrivate),
            "Backing field should be private",
            Literals.Category,
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor BackingFieldCannotBeAccessed = new(
            $"TRA{Literals.BackingFieldAccessAnalyzer_Id}02",
            nameof(BackingFieldCannotBeAccessed),
            "Cannot access a backing field here",
            Literals.Category,
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor TypeDoesnotContainsMember_0MemberName = new(
            $"TRA{Literals.BackingFieldAccessAnalyzer_Id}03",
            nameof(TypeDoesnotContainsMember_0MemberName),
            "Cannot find member {0} in type",
            Literals.Category,
            DiagnosticSeverity.Error,
            true);
    }
}
