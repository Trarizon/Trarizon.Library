using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using Trarizon.Library.Roslyn.Extensions;
using static Trarizon.Library.Generators.SingletonGenerator;

namespace Trarizon.Library.Generators;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class SingletonAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Diag.SingletonCtorIsNotAccessable);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterOperationAction(
            DoNotCallCtorManually,
            OperationKind.ObjectCreation);
    }

    private void DoNotCallCtorManually(OperationAnalysisContext context)
    {
        if (context.IsGeneratedCode)
            return;

        var operation = (IObjectCreationOperation)context.Operation;
        var ctorSymbol = operation.Constructor;
        if (!AttributeProxy.TryGet(ctorSymbol, out _))
            return;

        context.ReportDiagnostic(
            Diag.SingletonCtorIsNotAccessable,
            operation.Syntax.GetLocation());
    }
}
