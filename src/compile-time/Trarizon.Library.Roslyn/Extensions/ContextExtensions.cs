using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Roslyn.Collections.Comparisons;
using Trarizon.Library.Roslyn.Diagnostics;

namespace Trarizon.Library.Roslyn.Extensions;
public static partial class ContextExtensions
{
    public static IncrementalValueProvider<ImmutableArray<T>> WithImmutableArraySequenceComparer<T>(this IncrementalValueProvider<ImmutableArray<T>> provider)
        => provider.WithComparer(ImmutableArraySequenceEqualityComparer<T>.Default);

    public static IncrementalValuesProvider<ImmutableArray<T>> WithImmutableArraySequenceComparer<T>(this IncrementalValuesProvider<ImmutableArray<T>> provider)
        => provider.WithComparer(ImmutableArraySequenceEqualityComparer<T>.Default);

    public static IncrementalValuesProvider<T> OfNotNull<T>(this IncrementalValuesProvider<T?> provider) where T : class
    => provider.Where(x => x is not null)!;

    public static IncrementalValuesProvider<T> OfNotNull<T>(this IncrementalValuesProvider<T?> provider) where T : struct
        => provider.Where(x => x is not null).Select((x, _) => x!.Value);

    #region Diagnostics

    public static void ReportDiagnostic(this in OperationAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object[]? message)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));

    public static void ReportDiagnostic(this in SymbolAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object[]? message)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));

    public static void ReportDiagnostic(this in SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object[]? message)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));

    #endregion
}
