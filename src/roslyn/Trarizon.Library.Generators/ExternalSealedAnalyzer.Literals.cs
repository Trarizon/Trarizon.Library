using Microsoft.CodeAnalysis;
using Trarizon.Library.Roslyn.Extensions;

namespace Trarizon.Library.Generators;
partial class ExternalSealedAnalyzer
{
    private readonly struct AttributeProxy
    {
        public static bool IsThisType(AttributeData attribute)
            => attribute.AttributeClass.MatchDisplayString($"{Literals.NS_CodeAnalysis}.ExternalSealedAttribute");
    }

    private static class Diag
    {
        public readonly static DiagnosticDescriptor CannotInheritOrImplementType = new(
            $"TRA{Literals.ExternalSealedAnalyer_Id}01",
            nameof(CannotInheritOrImplementType),
            "Cannot inherit or implement this type outside its assembly",
            Literals.Category,
            DiagnosticSeverity.Error,
            true);
    }
}
