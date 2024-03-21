﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

        context.RegisterOperationAction(
            CheckAccessor,
            OperationKind.FieldReference);
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
        if (!(fieldFirstDeclaratorSymbol.DeclaredAccessibility is Accessibility.NotApplicable or Accessibility.Private)) {
            context.ReportDiagnostic(
                Literals.Diagnostic_BackingFieldShouldBePrivate,
                fieldSyntax.Declaration);
        }

        // Validate members
        attr.GetConstructorArguments<string>(Literals.Attribute_AccessableMembers_ConstructorIndex)
            .Where(member => !fieldFirstDeclaratorSymbol.ContainingType.MemberNames.Contains(member))
            .ForEach(notFoundMember =>
            {
                context.ReportDiagnostic(
                    Literals.Diagnostic_TypeDoesnotContainsMember_0MemberName,
                    fieldSyntax.Declaration,
                    notFoundMember);
            });
    }

    static void CheckAccessor(OperationAnalysisContext context)
    {
        var operation = (IFieldReferenceOperation)context.Operation;
        var symbol = operation.Field;

        var accessAttr = symbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass.MatchDisplayString(Literals.Attribute_TypeName));
        if (accessAttr is null)
            return;

        var accessableMembers = accessAttr.GetConstructorArguments<string>(Literals.Attribute_AccessableMembers_ConstructorIndex);

        bool accessable = operation.Syntax.Ancestors()
            .OfTypeUntil<MemberDeclarationSyntax, TypeDeclarationSyntax>()
            .Select(syntax => operation.SemanticModel.GetDeclaredSymbol(syntax)?.Name)
            .Intersect(accessableMembers)
            .Any();
        if (accessable)
            return;

        context.ReportDiagnostic(
           Literals.Diagnostic_BackingFieldCannotBeAccessed,
           operation.Syntax);
    }
}
