using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Trarizon.Library.GeneratorToolkit.Factories;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class AnalysisContextExtensions
{
    public static void ReportDiagnostic(this SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, in SyntaxNodeOrToken syntax)
        => context.ReportDiagnostic(DiagnosticFactory.Create(descriptor, syntax));
}
