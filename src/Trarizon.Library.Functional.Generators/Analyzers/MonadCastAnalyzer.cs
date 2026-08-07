using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using Trarizon.Library.Roslyn;

namespace Trarizon.Library.Functional.Generators.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class MonadCastAnalyzer : DiagnosticAnalyzer
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

            // Optional

            if (!compilation.TryGetTypeByMetadataName("Trarizon.Library.Functional.Optional`1", out var optionalTypeSymbol))
                return;
            var castMethodSymbol = optionalTypeSymbol.GetMembers("Cast")
                .OfType<IMethodSymbol>()
                .FirstOrDefault();
            if (castMethodSymbol is null)
                return;

            context.RegisterOperationAction(AnalysisAction(castMethodSymbol, (0, 0)), OperationKind.Invocation);

            // Result

            if (!compilation.TryGetTypeByMetadataName("Trarizon.Library.Functional.Result`2", out var resultTypeSymbol))
                return;

            var resultCastMethodSymbol = resultTypeSymbol.GetMembers("Cast")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(x => x.TypeParameters.Length == 1);
            if (resultCastMethodSymbol is not null)
            {
                context.RegisterOperationAction(AnalysisAction(resultCastMethodSymbol, (0, 0)), OperationKind.Invocation);
            }

            var resultCastMethodSymbol2 = resultTypeSymbol.GetMembers("Cast")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(x => x.TypeParameters.Length == 2);
            if (resultCastMethodSymbol2 is not null)
            {
                context.RegisterOperationAction(AnalysisAction(resultCastMethodSymbol2, (0, 0), (1, 1)), OperationKind.Invocation);
            }

            var resultCastMethodSymbol3 = resultTypeSymbol.GetMembers("CastError")
                .OfType<IMethodSymbol>()
                .FirstOrDefault();
            if (resultCastMethodSymbol3 is not null)
            {
                context.RegisterOperationAction(AnalysisAction(resultCastMethodSymbol3, (1, 0)), OperationKind.Invocation);
            }
        });
    }

    private Action<OperationAnalysisContext> AnalysisAction(IMethodSymbol castMethodSymbol, params (int Type, int Method)[] typeArgPairs)
    {
        return context =>
        {
            var operation = (IInvocationOperation)context.Operation;
            if (operation.Instance?.Type is not INamedTypeSymbol instanceType)
                return;

            if (SymbolEqualityComparer.Default.Equals(operation.TargetMethod.OriginalDefinition, castMethodSymbol))
            {
                foreach (var (typeTypeArg, methodTypeArg) in typeArgPairs)
                {
                    var fromType = instanceType.TypeArguments[typeTypeArg];
                    var toType = operation.TargetMethod.TypeArguments[methodTypeArg];

                    if (MaybeCastable(fromType, toType, context.Compilation) is not true)
                        context.ReportDiagnostic(CreateDiagnostic(operation, fromType, toType));
                }
            }
        };
    }

    private static bool? MaybeCastable(ITypeSymbol from, ITypeSymbol to, Compilation compilation)
    {
        if (from.TypeKind is TypeKind.Unknown or TypeKind.Error or TypeKind.TypeParameter)
            return null;
        if (to.TypeKind is TypeKind.Unknown or TypeKind.Error or TypeKind.TypeParameter)
            return null;

        var conversion = compilation.ClassifyConversion(from, to);
        if (conversion.IsIdentity || conversion.IsBoxing || conversion.IsUnboxing || conversion.IsReference)
            return true;
        return false;
    }

    private Diagnostic CreateDiagnostic(IInvocationOperation operation, ITypeSymbol from, ITypeSymbol to)
    {
        return Diagnostic.Create(InvalidCast, operation.Syntax.GetLocation(), from.ToDisplayString(), to.ToDisplayString());
    }
}