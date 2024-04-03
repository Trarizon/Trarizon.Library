using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class ContextExtensions
{
    public static void ReportDiagnostic(this SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, messageArgs));
    
    public static void ReportDiagnostic(this SymbolAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, messageArgs));
    
    public static void ReportDiagnostic(this OperationAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, messageArgs));
}
