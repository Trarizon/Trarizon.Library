using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.Library.Functional;
using Trarizon.Library.Roslyn.Extensions;
using static Trarizon.Library.CodeAnalysis.SourceGeneration.SingletonGenerator;

namespace Trarizon.Library.CodeAnalysis.SourceGeneration;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class SingletonAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = Descriptors.GetAnalyzerDiagnostics();

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

        Optional.OfNotNull(operation.Constructor)
            .Where(ctor => IsSingletonType(ctor.ContainingType))
            .MatchValue(_ => context.ReportDiagnostic(
                Descriptors.CallSingletonConstrucotrMauallyIsNotAllowed,
                operation.Syntax.GetLocation()));
    }

    private bool IsSingletonType(ITypeSymbol? symbol)
    {
        if (symbol is not INamedTypeSymbol type)
            return false;

        if (type.IsValueType || type.IsRecord)
            return false;

        if (type.IsAbstract || type.IsStatic)
            return false;

        return type.GetAttributes()
            .Any(attr => attr.AttributeClass.MatchDisplayString(RuntimeAttribute.TypeFullName));
    }
}
