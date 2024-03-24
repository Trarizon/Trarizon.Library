using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Text;
using Trarizon.Library.SourceGenerator.Toolkit.Factories;

namespace Trarizon.Library.SourceGenerator.Toolkit.Extensions;
public static class ContextExtensions
{
    public static void ReportDiagnostic(this in SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, in SyntaxNodeOrToken syntax, params object?[]? messageArgs)
        => context.ReportDiagnostic(DiagnosticFactory.Create(descriptor, syntax, messageArgs));

    public static void ReportDiagnostic(this in OperationAnalysisContext context, DiagnosticDescriptor descriptor, in SyntaxNodeOrToken syntax, params object?[]? messageArgs)
        => context.ReportDiagnostic(DiagnosticFactory.Create(descriptor, syntax, messageArgs));

    public static void ReportDiagnostic(this in SymbolAnalysisContext context, DiagnosticDescriptor descriptor, in SyntaxNodeOrToken syntax, params object?[]? messageArgs)
        => context.ReportDiagnostic(DiagnosticFactory.Create(descriptor, syntax, messageArgs));

    public static void ReportDiagnostic(this in SymbolAnalysisContext context, DiagnosticDescriptor descriptor, SyntaxReference syntax, params object?[]? messageArgs)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, Location.Create(syntax.SyntaxTree, syntax.Span), messageArgs));

    public static void AddSource(this in SourceProductionContext context, string hintName, CompilationUnitSyntax compilationUnit)
        => context.AddSource(hintName, compilationUnit.NormalizeWhitespace().GetText(Encoding.UTF8));
}
