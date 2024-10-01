using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.Library.GeneratorToolkit.ContextModelExtensions;
using Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
using static Trarizon.Library.SourceGeneration.Generators.SingletonLiterals;

namespace Trarizon.Library.SourceGeneration.Generators;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class SingletonAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Diagnostic_SingletonCtorIsNotAccessable];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        // Do not call ctor manually
        context.RegisterSyntaxNodeAction(context =>
        {
            var ctorAccessSyntax = (BaseObjectCreationExpressionSyntax)context.Node;
            var ctorSymbol = context.SemanticModel.GetSymbolInfo(ctorAccessSyntax).Symbol as IMethodSymbol;

            if (true != ctorSymbol?.ContainingType?.GetAttributes().Any(attr => attr.AttributeClass.MatchDisplayString(L_Attribute_TypeName)))
                return;

            var accessorMemberSyntax = ctorAccessSyntax.Ancestors()
                .OfType<MemberDeclarationSyntax>()
                .FirstOrDefault();
            if (accessorMemberSyntax is null)
                goto ReportDiagnostic;

            var accessorMemberSymbol = accessorMemberSyntax switch
            {
                FieldDeclarationSyntax field => context.SemanticModel.GetDeclaredSymbol(field.Declaration.Variables[0]),
                _ => context.SemanticModel.GetDeclaredSymbol(accessorMemberSyntax),
            };
            if (accessorMemberSymbol is null)
                return;

            // 排除singleton provider的调用
            var accessorIsGenerated = accessorMemberSymbol.EnumerateByWhileNotNull(s => s.ContainingSymbol)
                .Any(s => s.TryGetGeneratedCodeAttribute(Literals.GeneratedCodeAttribute_Tool_Argument, out var attributeData));
            if (accessorIsGenerated) {
                return;
            }

        ReportDiagnostic:
            context.ReportDiagnostic(
                Diagnostic_SingletonCtorIsNotAccessable,
                ctorAccessSyntax.GetLocation());

        }, SyntaxKind.ObjectCreationExpression, SyntaxKind.ImplicitObjectCreationExpression);
    }
}
