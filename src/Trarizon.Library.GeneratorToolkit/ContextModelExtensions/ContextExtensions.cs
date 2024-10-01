using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Trarizon.Library.GeneratorToolkit.CoreLib.Wrappers;
using Trarizon.Library.GeneratorToolkit.Wrappers;

namespace Trarizon.Library.GeneratorToolkit.ContextModelExtensions;
public static class ContextExtensions
{
    public static void ReportDiagnostic(this in SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object[]? message)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));

    public static void ReportDiagnostic(this in SymbolAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object[]? message)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));

    public static void ReportDiagnostic(this in OperationAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object[]? message)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));

    public static void RegisterSourceOutput<T>(this in IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<ParseResult<T>> result) where T : ISourceEmitter
    {
        context.RegisterSourceOutput(result, static (context, result) =>
        {
            foreach (var diag in result.DiagnosticDatas) {
                context.ReportDiagnostic(diag.ToDiagnostic());
            }
            if (result.Result.TryGetValue(out var emitter)) {
                context.AddSource(emitter.GenerateFileName(), emitter.Emit());
            }
        });
    }

    public static void RegisterSourceOutputAndPrintDebug<T>(this in IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<ParseResult<T>> result) where T : ISourceEmitter
    {
        context.RegisterSourceOutput(result, (context, result) =>
        {
            foreach (var diag in result.DiagnosticDatas) {
                context.ReportDiagnostic(diag.ToDiagnostic());
            }
            if (result.Result.TryGetValue(out var emitter)) {
                var res = emitter.Emit();
#if DEBUG
                Console.WriteLine(res);
#endif
                context.AddSource(emitter.GenerateFileName(), res);
            }
        });
    }
}
