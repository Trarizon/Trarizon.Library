using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Roslyn.Diagnostics;
using Trarizon.Library.Roslyn.Emitting;

namespace Trarizon.Library.Roslyn.Extensions;
public static class ContextExtensions
{
    public static void ReportDiagnostic(this in OperationAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object[]? message)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));

    public static void ReportDiagnostic(this in SymbolAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object[]? message)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));

    public static bool ReportDiagnosticWhenNotNull(this in SymbolAnalysisContext context, [NotNullWhen(false)] DiagnosticData? diagnostic)
    {
        if (diagnostic is not null) {
            context.ReportDiagnostic(diagnostic.ToDiagnostic());
            return true;
        }
        return false;
    }

    public static void ReportDiagnostic(this in SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object[]? message)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));
}
