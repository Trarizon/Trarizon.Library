using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Trarizon.Library.Functional.SourceGeneration.Analyzers;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class MonadsCastAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [InvalidCast];

    private static readonly DiagnosticDescriptor InvalidCast = new(
        "TRAFNL0001",
        "Invalid cast",
        "Cast type '{0}' to '{1}' may cause InvalidCastException",
        "Trarizon.Library.Functional",
        DiagnosticSeverity.Warning,
        true);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(context =>
        {
            var compilation = context.Compilation;

            var optionalTypeSymbol = compilation.GetTypeByMetadataName("Trarizon.Library.Functional.Optional`1");
            if (optionalTypeSymbol is null)
                return;
            var castMethodSymbol = optionalTypeSymbol.GetMembers("Cast")
                .OfType<IMethodSymbol>()
                .First();

            context.RegisterOperationAction(context =>
            {
                var operation = (IInvocationOperation)context.Operation;
                if (operation.Instance?.Type is not INamedTypeSymbol instanceType)
                    return;

                if (SymbolEqualityComparer.Default.Equals(operation.TargetMethod.OriginalDefinition, castMethodSymbol)) {
                    var fromType = instanceType.TypeArguments[0];
                    var toType = operation.TargetMethod.TypeArguments[0];
                    if (MaybeCastable(fromType, toType, compilation))
                        return;
                    context.ReportDiagnostic(GetDiagnostic(operation, fromType, toType));
                    return;
                }
            }, OperationKind.Invocation);
        });

        context.RegisterCompilationStartAction(context =>
        {
            var compilation = context.Compilation;
            var resultSymbol = compilation.GetTypeByMetadataName("Trarizon.Library.Functional.Result`2");
            if (resultSymbol is null)
                return;
            var resultCastSymbol = resultSymbol.GetMembers("Cast")
                .OfType<IMethodSymbol>()
                .Where(m => m.TypeParameters.Length == 1)
                .First();
            var resultCast2Symbol = resultSymbol.GetMembers("Cast")
                .OfType<IMethodSymbol>()
                .Where(m => m.TypeParameters.Length == 2)
                .First();
            var resultCastErrorSymbol = resultSymbol.GetMembers("CastError")
                .OfType<IMethodSymbol>()
                .First();

            context.RegisterOperationAction(context =>
            {
                var operation = (IInvocationOperation)context.Operation;
                if (operation.Instance?.Type is not INamedTypeSymbol instanceType)
                    return;
                var invokeMethod = operation.TargetMethod.OriginalDefinition;

                if (SymbolEqualityComparer.Default.Equals(invokeMethod, resultCastSymbol)) {
                    var fromType = instanceType.TypeArguments[0];
                    var toType = operation.TargetMethod.TypeArguments[0];
                    if (MaybeCastable(fromType, toType, compilation))
                        return;
                    context.ReportDiagnostic(GetDiagnostic(operation, fromType, toType));
                    return;
                }
                if (SymbolEqualityComparer.Default.Equals(invokeMethod, resultCastErrorSymbol)) {
                    var fromType = instanceType.TypeArguments[1];
                    var toType = operation.TargetMethod.TypeArguments[0];
                    if (MaybeCastable(fromType, toType, compilation))
                        return;
                    context.ReportDiagnostic(GetDiagnostic(operation, fromType, toType));
                    return;
                }
                if (SymbolEqualityComparer.Default.Equals(invokeMethod, resultCast2Symbol)) {
                    var from1 = instanceType.TypeArguments[0];
                    var from2 = instanceType.TypeArguments[1];
                    var to1 = operation.TargetMethod.TypeArguments[0];
                    var to2 = operation.TargetMethod.TypeArguments[1];
                    if (!MaybeCastable(from1, to1, compilation))
                        context.ReportDiagnostic(GetDiagnostic(operation, from1, to1));
                    if (!MaybeCastable(from2, to2, compilation))
                        context.ReportDiagnostic(GetDiagnostic(operation, from2, to2));
                    return;
                }
            }, OperationKind.Invocation);
        });
    }

    private static bool MaybeCastable(ITypeSymbol fromType, ITypeSymbol toType, Compilation compilation)
    {
        if (fromType.TypeKind is TypeKind.Unknown or TypeKind.Error or TypeKind.TypeParameter)
            return true;
        if (toType.TypeKind is TypeKind.Unknown or TypeKind.Error or TypeKind.TypeParameter)
            return true;

        var conversion = compilation.ClassifyConversion(fromType, toType);
        if (conversion.IsIdentity || conversion.IsBoxing || conversion.IsUnboxing || conversion.IsReference)
            return true;
        return false;
    }

    private Diagnostic GetDiagnostic(IInvocationOperation operation, ITypeSymbol fromType, ITypeSymbol toType)
    {
        return Diagnostic.Create(
            InvalidCast,
            operation.Syntax.GetLocation(),
            fromType.ToDisplayString(),
            toType.ToDisplayString());
    }
}
