using Microsoft.CodeAnalysis;

namespace Trarizon.Library.SourceGenerator.Diagnostics;
internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor Optional_ValueIsAlwaysDefaultIfHasNoValue = new(
        "TRALIB0001",
        nameof(Optional_ValueIsAlwaysDefaultIfHasNoValue),
        "Value is always default if Optional<T> doesn't have value, use discard(_)",
        "Optional<T>",
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor Optional_OnlyBooleanConstantAllowedAtFirstPosition = new(
        "TRALIB0002",
        nameof(Optional_OnlyBooleanConstantAllowedAtFirstPosition),
        "The first param only allows constant boolean value, if you want to get the value, use Optional<T>.HasValue, or directly call Optional<T>.Deconstruct",
        "Optional<T>",
        DiagnosticSeverity.Error,
        true);
}
