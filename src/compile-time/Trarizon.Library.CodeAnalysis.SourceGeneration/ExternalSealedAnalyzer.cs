using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Trarizon.Library.CodeAnalysis.SourceGeneration.Literals;
using Trarizon.Library.Collections;
using Trarizon.Library.Roslyn.Extensions;

namespace Trarizon.Library.CodeAnalysis.SourceGeneration;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed partial class ExternalSealedAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Diag];

    private static readonly DiagnosticDescriptor Diag = new(
        DiagnosticIds.ExternalSealed + "00",
        "Type is external sealed",
        "Cannot inherit from external sealed type",
        Categories.AccessControl,
        DiagnosticSeverity.Error, true);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(context =>
        {
            var compilation = context.Compilation;
            if (!compilation.TryGetExternalSealedAttribute(out var externalSealedAttributeSymbol))
                return;

            context.RegisterSymbolAction(context =>
            {
                var symbol = (INamedTypeSymbol)context.Symbol;

                if (symbol.BaseType?.HasAttribute(externalSealedAttributeSymbol) ?? false) {
                    context.ReportDiagnostic(Diag, symbol.Locations[0]);
                }

            }, SymbolKind.NamedType);
        });
    }
}
