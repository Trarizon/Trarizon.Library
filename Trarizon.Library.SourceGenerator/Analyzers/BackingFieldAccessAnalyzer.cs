using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.Library.SourceGenerator.Toolkit;
using Trarizon.Library.SourceGenerator.Toolkit.Extensions;

namespace Trarizon.Library.SourceGenerator.Analyzers;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal partial class BackingFieldAccessAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Literals.Diagnostic_BackingFieldCannotBeAccessed,
        Literals.Diagnostic_BackingFieldShouldBePrivate,
        Literals.Diagnostic_TypeDoesnotContainsMember_0MemberName);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        Dictionary<ISymbol, AttributeData> _attrDict = new(SymbolEqualityComparer.Default);

        context.RegisterSyntaxNodeAction(
            FieldShouldBePrivate_ValidateAccessorMemberName,
            SyntaxKind.FieldDeclaration);

        context.RegisterSyntaxNodeAction(
            CheckAccessor,
            SyntaxKind.IdentifierName);
    }

    static void FieldShouldBePrivate_ValidateAccessorMemberName(SyntaxNodeAnalysisContext context)
    {
        var fieldSyntax = (FieldDeclarationSyntax)context.Node;
        var fieldFirstDeclaratorSymbol = context.SemanticModel.GetDeclaredSymbol(fieldSyntax.Declaration.Variables[0]);
        if (fieldFirstDeclaratorSymbol is null)
            return;

        var attr = fieldFirstDeclaratorSymbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass.MatchDisplayString(Literals.Attribute_TypeName));
        if (attr is null)
            return;

        // field is private
        if (!(fieldSyntax.GetAccessModifiers() is AccessModifiers.None or AccessModifiers.Private)) {
            context.ReportDiagnostic(
                Literals.Diagnostic_BackingFieldShouldBePrivate,
                fieldSyntax.Declaration);
        }

        // Validate members
        var notFoundMembers = attr.GetConstructorArguments<string>(Literals.Attribute_AccessableMembers_ConstructorIndex)
            .Where(member => !fieldFirstDeclaratorSymbol.ContainingType.MemberNames.Contains(member));
        foreach (var nfMember in notFoundMembers) {
            context.ReportDiagnostic(
                Literals.Diagnostic_TypeDoesnotContainsMember_0MemberName,
                fieldSyntax.Declaration,
                nfMember);
        }
    }

    static void CheckAccessor(SyntaxNodeAnalysisContext context)
    {
        var identifierNameSyntax = (IdentifierNameSyntax)context.Node;
        var fieldSymbol = context.SemanticModel.GetSymbolInfo(identifierNameSyntax).Symbol;

        var accessAttr = fieldSymbol?.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass.MatchDisplayString(Literals.Attribute_TypeName));
        if (accessAttr is null)
            return;

        var accessableMembers = accessAttr.GetConstructorArguments<string>(Literals.Attribute_AccessableMembers_ConstructorIndex);

        bool accessable = identifierNameSyntax.Ancestors()
            .OfType<MemberDeclarationSyntax>()
            .TakeWhile(decl => decl is not TypeDeclarationSyntax)
            .Select(syntax => context.SemanticModel.GetDeclaredSymbol(syntax))
            .CartesianProduct(accessableMembers)
            .Any(tuple => tuple.Item1?.Name == tuple.Item2);
        if (accessable)
            return;

        context.ReportDiagnostic(
            Literals.Diagnostic_BackingFieldCannotBeAccessed,
            identifierNameSyntax.Identifier);
    }
}
