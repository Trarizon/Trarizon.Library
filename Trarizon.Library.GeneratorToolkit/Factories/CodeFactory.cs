using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Trarizon.Library.GeneratorToolkit.Extensions;

namespace Trarizon.Library.GeneratorToolkit.Factories;
/// <summary>
/// Extensions of <see cref="SyntaxFactory"/>,
/// </summary>
public static class CodeFactory
{
    public static SyntaxTokenList Modifiers(ReadOnlySpan<SyntaxKind> modifiers)
    {
        var tokens = new SyntaxToken[modifiers.Length];
        for (int i = 0; i < modifiers.Length; i++) {
            tokens[i] = SyntaxFactory.Token(modifiers[i]);
        }
        return SyntaxFactory.TokenList(tokens);
    }

    public static SyntaxTokenList Modifiers(SyntaxKind modifier)
        => SyntaxFactory.TokenList(SyntaxFactory.Token(modifier));

    #region Clone

    #region Clone partial declaration

    /// <summary>
    /// Create a partial type by copy the basic info of original type
    /// </summary>
    public static TypeDeclarationSyntax ClonePartialDeclaration(TypeDeclarationSyntax source,
        SyntaxList<AttributeListSyntax> attributeLists,
        BaseListSyntax? baseList,
        SyntaxList<MemberDeclarationSyntax> members)
        => source switch {
            ClassDeclarationSyntax clz => ClonePartialDeclaration(clz, attributeLists, baseList, members),
            StructDeclarationSyntax str => ClonePartialDeclaration(str, attributeLists, baseList, members),
            RecordDeclarationSyntax rec => ClonePartialDeclaration(rec, attributeLists, baseList, members),
            InterfaceDeclarationSyntax itf => ClonePartialDeclaration(itf, attributeLists, baseList, members),
            _ => throw new InvalidOperationException("Unknown type declaration"),
        };

    /// <summary>
    /// Create a partial class by copy the basic info of original class
    /// </summary>
    public static ClassDeclarationSyntax ClonePartialDeclaration(ClassDeclarationSyntax source,
        SyntaxList<AttributeListSyntax> attributeLists,
        BaseListSyntax? baseList,
        SyntaxList<MemberDeclarationSyntax> members)
        => SyntaxFactory.ClassDeclaration(
            attributeLists,
            Modifiers(SyntaxKind.PartialKeyword),
            source.Identifier,
            source.TypeParameterList,
            baseList,
            default,
            members);

    /// <summary>
    /// Create a partial struct by copy the basic info of original struct
    /// </summary>
    public static StructDeclarationSyntax ClonePartialDeclaration(StructDeclarationSyntax source,
        SyntaxList<AttributeListSyntax> attributeLists,
        BaseListSyntax? baseList,
        SyntaxList<MemberDeclarationSyntax> members)
        => SyntaxFactory.StructDeclaration(
            attributeLists,
            Modifiers(SyntaxKind.PartialKeyword),
            source.Identifier,
            source.TypeParameterList,
            baseList,
            default,
            members);

    /// <summary>
    /// Create a partial interface by copy the basic info of original interface
    /// </summary>
    public static InterfaceDeclarationSyntax ClonePartialDeclaration(InterfaceDeclarationSyntax source,
        SyntaxList<AttributeListSyntax> attributeLists,
        BaseListSyntax? baseList,
        SyntaxList<MemberDeclarationSyntax> members)
        => SyntaxFactory.InterfaceDeclaration(
            attributeLists,
            Modifiers(SyntaxKind.PartialKeyword),
            source.Identifier,
            source.TypeParameterList,
            baseList,
            default,
            members);

    /// <summary>
    /// Create a partial record by copy the basic info of original record
    /// </summary>
    public static RecordDeclarationSyntax ClonePartialDeclaration(RecordDeclarationSyntax source,
        SyntaxList<AttributeListSyntax> attributeLists,
        BaseListSyntax? baseList,
        SyntaxList<MemberDeclarationSyntax> members)
        => SyntaxFactory.RecordDeclaration(
            attributeLists,
            Modifiers(SyntaxKind.PartialKeyword),
            SyntaxFactory.Token(SyntaxKind.RecordKeyword),
            source.Identifier,
            source.TypeParameterList,
            default,
            baseList,
            default,
            members);

    public static MethodDeclarationSyntax ClonePartialDeclaration(MethodDeclarationSyntax source,
        SyntaxList<AttributeListSyntax> attributeLists,
        BlockSyntax? body,
        ArrowExpressionClauseSyntax? expressionBody)
        => SyntaxFactory.MethodDeclaration(
            attributeLists,
            Modifiers(SyntaxKind.PartialKeyword),
            source.ReturnType,
            source.ExplicitInterfaceSpecifier,
            source.Identifier,
            source.TypeParameterList,
            source.ParameterList,
            source.ConstraintClauses,
            body,
            expressionBody,
            expressionBody is not null ? SyntaxFactory.Token(SyntaxKind.SemicolonToken) : default);

    #endregion

    /// <summary>
    /// Create all containing partial types of a type by copy the basic info of original types
    /// </summary>
    public static TypeDeclarationSyntax CloneContainingTypeDeclarations(TypeDeclarationSyntax sourceSyntax,
        SyntaxList<MemberDeclarationSyntax> members)
    {
        TypeDeclarationSyntax type = ClonePartialDeclaration(sourceSyntax, default, default, members);

        while (sourceSyntax.GetParent<TypeDeclarationSyntax>() is { } sourceParent) {
            sourceSyntax = sourceParent;
            type = ClonePartialDeclaration(sourceSyntax, default, default,
                SyntaxFactory.SingletonList<MemberDeclarationSyntax>(type));
        }

        return type;
    }

    /// <summary>
    /// Create containing partial types and namespace from given source
    /// </summary>
    public static MemberDeclarationSyntax CloneContainingTypeAndNamespaceDeclarations(TypeDeclarationSyntax sourceTypeSyntax, ISymbol sourceMemberSymbol,
        SyntaxList<MemberDeclarationSyntax> members)
    {
        var topType = CloneContainingTypeDeclarations(sourceTypeSyntax, members);

        string nsString = sourceMemberSymbol.ContainingNamespace.ToDisplayString();
        if (nsString == Literals.GlobalNamespaceDisplayString)
            return topType;

        return SyntaxFactory.NamespaceDeclaration(
            SyntaxFactory.ParseName(nsString),
            externs: default,
            usings: default,
            members: SyntaxFactory.SingletonList<MemberDeclarationSyntax>(topType));
    }

    #endregion

    #region [GeneratedCode]

    public static AttributeListSyntax GetGeneratedCodeAttributeSyntax(string tool, string version) => SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
        SyntaxFactory.Attribute(
            SyntaxFactory.ParseName($"global::{Literals.GeneratedCodeAttributeFullName}"),
            SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
                SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal(tool))),
                SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal(version)))
            ])))));

    #endregion
}
