using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Trarizon.Library.GeneratorToolkit.Factories;
using Trarizon.Library.GeneratorToolkit.Wrappers;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class AnalysisContextExtensions
{
    public static void ReportDiagnostic(this SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, in Either<SyntaxNode, SyntaxToken> syntax)
        => context.ReportDiagnostic(DiagnosticFactory.Create(descriptor, syntax));
}
