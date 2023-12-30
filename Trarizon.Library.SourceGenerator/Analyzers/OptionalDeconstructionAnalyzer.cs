using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Trarizon.Library.GeneratorToolkit.Extensions;
using Trarizon.Library.GeneratorToolkit.Factories;
using static Trarizon.Library.SourceGenerator.Diagnostics.DiagnosticDescriptors;

namespace Trarizon.Library.SourceGenerator.Analyzers;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OptionalDeconstructionAnalyzer : DiagnosticAnalyzer
{
    private const string OptionalTypeNameWithoutGenericArguments = "Trarizon.Library.Wrappers.Optional";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Optional_ValueIsAlwaysDefaultIfHasNoValue,
        Optional_OnlyBooleanConstantAllowedAtFirstPosition);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(context =>
        {
            var positionalPatternClauseSyntax = (PositionalPatternClauseSyntax)context.Node;

            // Ensure the expression type is Optional<T>

            var expressionSyntax = positionalPatternClauseSyntax.Parent?.Parent switch {
                IsPatternExpressionSyntax isPatternExpressionSyntax => isPatternExpressionSyntax.Expression,
                SwitchExpressionArmSyntax switchExpressionArmSyntax => switchExpressionArmSyntax.GetParent<SwitchExpressionSyntax>()?.GoverningExpression,
                CasePatternSwitchLabelSyntax casePatternSwitchLabelSyntax => casePatternSwitchLabelSyntax.GetParent<SwitchStatementSyntax>()?.Expression,
                _ => default,
            };
            if (expressionSyntax is null)
                return;
            var type = context.SemanticModel.GetTypeInfo(expressionSyntax).Type;
            if (!IsOptionalType(type))
                return;

            // Validate positional clause

            var subpatterns = positionalPatternClauseSyntax.Subpatterns;
            if (subpatterns.Count != 2) // Impossible
                return;

            if (subpatterns[0] is var first &&
                first.Pattern is not ConstantPatternSyntax constantPatternSyntax) {
                context.ReportDiagnostic(DiagnosticFactory.Create(
                    Optional_OnlyBooleanConstantAllowedAtFirstPosition,
                    first));
                return;
            }

            var hasValue = (bool)context.SemanticModel.GetConstantValue(constantPatternSyntax.Expression).Value!;
            if (!hasValue && subpatterns[1] is var second &&
                second.Pattern is not DiscardPatternSyntax) {
                context.ReportDiagnostic(DiagnosticFactory.Create(
                    Optional_ValueIsAlwaysDefaultIfHasNoValue,
                    second));
            }

            // If hasValue, the second pattern doesn't require limitation

        }, SyntaxKind.PositionalPatternClause);
    }

    private static bool IsOptionalType(ITypeSymbol? type)
    {
        if (type is null)
            return false;

        var displayStr = type.ToDisplayString();
        if (displayStr[^1] != '>') // Optional<T> is generic
            return false;

        int i = displayStr.Length - 2;
        for (; i >= 0; i--) {
            var c = displayStr[i];
            if (c == ',') // Optional<T> has only one generic parameter
                return false;
            else if (c == '<') // Always run this branch
                break;
        }

        var typeNameWithoutGenericArgs = displayStr[..i];
        return typeNameWithoutGenericArgs == OptionalTypeNameWithoutGenericArguments;
    }
}
