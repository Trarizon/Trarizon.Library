using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.Library.GeneratorToolkit.ContextModelExtensions;
using static Trarizon.Library.SourceGeneration.Analyzers.ExternalSealedLiterals;

namespace Trarizon.Library.SourceGeneration.Analyzers;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class ExternalSealedAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = [CannotInheritOrImplementType];

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

        foreach (var baseTypeSyntax in typeSyntax.BaseList?.Types ?? []) {
            var btsymbol = context.SemanticModel.GetTypeInfo(baseTypeSyntax.Type).Type;
            if (btsymbol is null)
                continue;
            if (SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, btsymbol.ContainingAssembly))
                continue;
            if (IsSealed(btsymbol)) {
                context.ReportDiagnostic(
                    CannotInheritOrImplementType,
                    baseTypeSyntax.GetLocation());
            }
        }

        static bool IsSealed(ISymbol symbol)
            => symbol.GetAttributes().Any(attr => attr.AttributeClass.MatchDisplayString(L_Attribute_TypeName));
    }
}