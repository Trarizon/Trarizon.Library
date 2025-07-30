using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;
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

        context.RegisterSyntaxNodeAction(
            Validate,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration);
    }

    private void Validate(SyntaxNodeAnalysisContext context)
    {
        var typeSyntax = (TypeDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(typeSyntax, context.CancellationToken);

        if (symbol is null)
            return;

        typeSyntax.BaseList?.Types
            .Select(s => (s, context.SemanticModel.GetTypeInfo(s.Type).Type!))
            .Where(tpl => tpl.Item2 is not null)
            .Where(tpl => !SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, tpl.Item2.ContainingAssembly))
            .Where(tpl => IsExternalSealed(tpl.Item2))
            .ForEach(tpl => context.ReportDiagnostic(Diag, tpl.s.GetLocation()));
    }

    private static bool IsExternalSealed(ITypeSymbol symbol)
        => symbol.GetAttributes().Any(attr => attr.AttributeClass.MatchDisplayString($"{Namespaces.TrarizonDiagnostics}.ExternalSealedAttribute"));
}
