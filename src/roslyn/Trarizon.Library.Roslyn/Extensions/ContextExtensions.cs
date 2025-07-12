using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using Trarizon.Library.Roslyn.Emitting;

namespace Trarizon.Library.Roslyn.Extensions;
public static class ContextExtensions
{
    public static void ReportDiagnostic(this in OperationAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object[]? message)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));

    public static void ReportDiagnostic(this in SymbolAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object[]? message)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));

    public static void ReportDiagnostic(this in SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object[]? message)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));

    public static void RegisterSourceOutput<T>(this in IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<DiagnosticResult<T>> result) where T : ISourceEmitterWithIndentedWriter
    {
        context.RegisterSourceOutput(result, static (context, result) =>
        {
            foreach (var diag in result.DiagnosticDatas) {
                context.ReportDiagnostic(diag.ToDiagnostic());
            }
            if (result.Result.TryGetValue(out var emitter)) {
                context.AddSource(emitter.GeneratedFileName, emitter.Emit());
            }
        });
    }

    public static void RegisterSourceOutputAndPrintDebug<T>(this in IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<DiagnosticResult<T>> result) where T : ISourceEmitterWithIndentedWriter
    {
        context.RegisterSourceOutput(result, static (context, result) =>
        {
            foreach (var diag in result.DiagnosticDatas) {
                context.ReportDiagnostic(diag.ToDiagnostic());
            }
            if (result.Result.TryGetValue(out var emitter)) {
                var res = emitter.Emit();
#if DEBUG
                Console.WriteLine(res);
#endif
                context.AddSource(emitter.GeneratedFileName, res);
            }
        });
    }
}
