using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Immutable;

namespace Trarizon.Library.SourceGenerator.Analyzers;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal partial class ExplicitCaptureAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => throw new NotImplementedException();

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterOperationAction(
            default,
            OperationKind.LocalReference | OperationKind.ParameterReference);
    }

    private static void CheckReference(OperationAnalysisContext context)
    {
        
        //switch (context.Operation) {
        //    case ILocalReferenceOperation localReference: {
        //        localReference.Local.con
        //        break;
        //    }
        //    default:
        //        break;
        //}
    }
}
