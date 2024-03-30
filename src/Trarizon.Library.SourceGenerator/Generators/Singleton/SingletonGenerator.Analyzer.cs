using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.Library.SourceGenerator.Toolkit.Extensions;

namespace Trarizon.Library.SourceGenerator.Generators;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
partial class SingletonGenerator : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Diagnostic_SingletonCtorIsNotAccessable);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

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

            var accessorMemberSymbol = accessorMemberSyntax switch {
                FieldDeclarationSyntax field => context.SemanticModel.GetDeclaredSymbol(field.Declaration.Variables[0]),
                _ => context.SemanticModel.GetDeclaredSymbol(accessorMemberSyntax),
            };
            if (accessorMemberSymbol is null)
                return;

            // 排除singleton provider的调用
            if (accessorMemberSymbol.TryGetGeneratedCodeAttribute(out var attributeData) &&
                attributeData.GetConstructorArgument<string>(GlobalLiterals.GeneratedCodeAttribute_Tool_ConstructorIndex) is L_GeneratedCodeAttribute_Tool_Argument
                ) {
                return;
            }

        ReportDiagnostic:
            context.ReportDiagnostic(
                Diagnostic_SingletonCtorIsNotAccessable,
                ctorAccessSyntax);

        }, SyntaxKind.ObjectCreationExpression, SyntaxKind.ImplicitObjectCreationExpression);
    }
}
