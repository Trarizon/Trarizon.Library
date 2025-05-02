using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.Library.Collections;
using Trarizon.Library.Roslyn.Extensions;

namespace Trarizon.Library.Generators;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal partial class BackingFieldAccessAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Diag.BackingFieldShouldBePrivate,
            Diag.BackingFieldCannotBeAccessed,
            Diag.TypeDoesnotContainsMember_0MemberName);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(
            FieldShouldBePrivate_ValidateAccessorMemberName,
            SyntaxKind.FieldDeclaration);

        context.RegisterOperationAction(
            CheckAccessor,
            OperationKind.FieldReference);
    }

    private static void FieldShouldBePrivate_ValidateAccessorMemberName(SyntaxNodeAnalysisContext context)
    {
        var fieldSyntax = (FieldDeclarationSyntax)context.Node;
        var fieldFirstDeclaracterSymbol = context.SemanticModel.GetDeclaredSymbol(fieldSyntax.Declaration.Variables[0]);
        if (fieldFirstDeclaracterSymbol is null)
            return;

        if (!AttributeProxy.TryGet(fieldFirstDeclaracterSymbol, out var attr))
            return;

        // field is private

        if (fieldFirstDeclaracterSymbol.DeclaredAccessibility is not (Accessibility.NotApplicable or Accessibility.Private)) {
            context.ReportDiagnostic(
                Diag.BackingFieldShouldBePrivate,
                fieldSyntax.Declaration.GetLocation());
        }

        // Validate member

        var notFoundMembers = fieldFirstDeclaracterSymbol.ContainingType.MemberNames
            .Except(attr.AccessableMembers);
        if (notFoundMembers.Any()) {
            context.ReportDiagnostic(
                Diag.TypeDoesnotContainsMember_0MemberName,
                fieldSyntax.Declaration.GetLocation(),
                string.Join(", ", notFoundMembers));
        }
    }

    private static void CheckAccessor(OperationAnalysisContext context)
    {
        var operation = (IFieldReferenceOperation)context.Operation;
        var symbol = operation.Field;

        if (!AttributeProxy.TryGet(symbol, out var attr))
            return;

        bool accessable = operation.Syntax.Ancestors()
            .OfType<MemberDeclarationSyntax>()
            .OfTypeUntil<MemberDeclarationSyntax, TypeDeclarationSyntax>()
            .Select(syntax => operation.SemanticModel.GetDeclaredSymbol(syntax)?.Name)
            .Intersect(attr.AccessableMembers.AsEnumerable())
            .Any();
        if (accessable)
            return;

        context.ReportDiagnostic(
            Diag.BackingFieldCannotBeAccessed,
            operation.Syntax.GetLocation());
    }
}
