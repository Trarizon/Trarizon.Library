using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Trarizon.Library.Roslyn.Extensions;
public static class ContextExtensions
{
    public static void ReportDiagnostic(this in SymbolAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object[]? message) 
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));

    public static void ReportDiagnostic(this in OperationAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object[]? message)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));
}
