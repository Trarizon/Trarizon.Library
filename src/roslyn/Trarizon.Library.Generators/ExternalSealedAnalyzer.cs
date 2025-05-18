using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.Library.Collections;
using Trarizon.Library.Roslyn.Extensions;

namespace Trarizon.Library.Generators;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal partial class ExternalSealedAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Diag.CannotInheritOrImplementType);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(
            Validate,
            SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.RecordDeclaration, SyntaxKind.RecordStructDeclaration);
    }

    private void Validate(SyntaxNodeAnalysisContext context)
    {
        var typeSyntax = (TypeDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(typeSyntax);
        if (symbol is null)
            return;

        (typeSyntax.BaseList?.Types ?? SeparatedSyntaxList.Create<BaseTypeSyntax>(default))
            .Select(syntax => (syntax, context.SemanticModel.GetTypeInfo(syntax.Type).Type!))
            .Where(tpl => tpl.Item2 is not null)
            .Where(tpl => !SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, tpl.Item2.ContainingAssembly))
            .Where(tpl => tpl.Item2.GetAttributes().Any(AttributeProxy.IsThisType))
            .ForEach(tpl =>
            {
                context.ReportDiagnostic(
                    Diag.CannotInheritOrImplementType,
                    tpl.syntax.GetLocation());
            });
    }
}
