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
        IncrementalValuesProvider<Filter<TContext>> filtered,
        Action<SourceProductionContext, Filter<TContext>> action)
    {
        // Report diagnostics
        context.RegisterSourceOutput(
            filtered.SelectMany((src, token) => src.Diagnostics ?? []),
            static (context, diagnostic) => context.ReportDiagnostic(diagnostic));

        // Add source
        context.RegisterSourceOutput(filtered.Where(src => !src.HasDiagnostic), action);
    }

    public static Filter<StrongTypeAttributeSyntaxContext<TSyntax, TSymbol>>? ToFilteringResult<TSyntax, TSymbol>(this in GeneratorAttributeSyntaxContext context)
        where TSyntax : CSharpSyntaxNode
        where TSymbol : ISymbol
    {
        if (context is { TargetNode: TSyntax, TargetSymbol: TSymbol })
            return new Filter<StrongTypeAttributeSyntaxContext<TSyntax, TSymbol>>(new StrongTypeAttributeSyntaxContext<TSyntax, TSymbol>(context));
        else
            return null;
    }
}
