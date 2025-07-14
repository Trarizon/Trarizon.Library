using Microsoft.CodeAnalysis;
using System;
using Trarizon.Library.Roslyn.Diagnostics;

namespace Trarizon.Library.Roslyn.Emitting;
public static class SourceEmittingExtensions
{
    public static void RegisterSourceOutput<T>(this in IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<DiagnosticResult<T>> result) where T : ISourceEmitter
    {
        context.RegisterSourceOutput(result, static (context, result) =>
        {
            foreach (var diag in result.Diagnostics) {
                context.ReportDiagnostic(diag.ToDiagnostic());
            }
            if (result.ValueOptional.TryGetValue(out var emitter)) {
                context.AddSource(emitter.GeneratedFileName, emitter.Emit());
            }
        });
    }

    public static void RegisterSourceOutputAndPrintOnConsole<T>(this in IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<DiagnosticResult<T>> result) where T : ISourceEmitter
    {
        context.RegisterSourceOutput(result, static (context, result) =>
        {
            foreach (var diag in result.Diagnostics) {
                context.ReportDiagnostic(diag.ToDiagnostic());
            }
            if (result.ValueOptional.TryGetValue(out var emitter)) {
                var res = emitter.Emit();
                Console.WriteLine(res);
                context.AddSource(emitter.GeneratedFileName, res);
            }
        });
    }
}
