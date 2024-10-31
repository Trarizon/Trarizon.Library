using Microsoft.CodeAnalysis;

namespace Trarizon.Library.SourceGeneration.Analyzers;
internal static class ExternalSealedLiterals
{
    public const string L_Attribute_TypeName = $"{Literals.CodeAnalysis_Namespace}.ExternalSealedAttribute";

    public readonly static DiagnosticDescriptor CannotInheritOrImplementType = new(
        $"TRA{Literals.ExternalSealedAnalyer_Id}0001",
        nameof(CannotInheritOrImplementType),
        "Cannot inherit or implement this type outside its assembly",
        Literals.Category,
        DiagnosticSeverity.Error,
        true);
}
