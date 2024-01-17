using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Text;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class GeneratorContextExtensions
{
    public static void AddSource<TCompilationUnitProvider>(this in SourceProductionContext context,
        string hintName, TCompilationUnitProvider compilationUnitProvider)
        where TCompilationUnitProvider : ICompilationUnitProvider
    {
        context.AddSource(hintName, compilationUnitProvider.GetCompilationUnitSyntax().NormalizeWhitespace().GetText(Encoding.UTF8));
    }

    public static void RegisterFilteredSourceOutput<TContext>(this in IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<Filter<TContext>> filterSource,
        Action<SourceProductionContext, TContext> action)
    {
        context.RegisterSourceOutput(filterSource, (context, source) =>
        {
            source.OnFinal(
                con => action(context, con),
                diag => context.ReportDiagnostic(diag));
        });
    }
}
