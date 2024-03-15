using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Linq;
using Trarizon.Library.SourceGenerator.Toolkit.Extensions;

namespace Trarizon.Library.SourceGenerator.Toolkit.Factories;
public static class SyntaxProvider
{
    #region LiteralExpressions

    public static LiteralExpressionSyntax LiteralStringExpression(string value)
        => SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            SyntaxFactory.Literal(value));

    #endregion

    #region Factory patch

    public static AccessorDeclarationSyntax AccessorDeclarationNonBody(SyntaxKind accessorDeclarationKind)
        => SyntaxFactory.AccessorDeclaration(accessorDeclarationKind)
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

    #endregion

    #region Partial cloning

    #region Clone partial declaration

    /// <summary>
    /// Create a partial type by copy the basic info of original type
    /// </summary>
    public static TypeDeclarationSyntax ClonePartialDeclaration(TypeDeclarationSyntax source,
        SyntaxList<AttributeListSyntax> attributeLists,
        BaseListSyntax? baseList,
        SyntaxList<MemberDeclarationSyntax> members)
        => source switch
        {
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
            SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword)),
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
            SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword)),
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
            SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword)),
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
            SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword)),
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
            SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword)),
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

        return sourceSyntax.Ancestors().OfType<TypeDeclarationSyntax>()
            .Aggregate(type, (resType, type) => ClonePartialDeclaration(type, default, default, SyntaxFactory.SingletonList<MemberDeclarationSyntax>(resType)));
    }

    /// <summary>
    /// Create containing partial types and namespace from given source
    /// </summary>
    public static MemberDeclarationSyntax CloneContainingTypeAndNamespaceDeclarations(TypeDeclarationSyntax sourceTypeSyntax, ISymbol sourceMemberSymbol,
        SyntaxList<MemberDeclarationSyntax> members)
    {
        var topType = CloneContainingTypeDeclarations(sourceTypeSyntax, members);

        if (sourceMemberSymbol.ContainingNamespace.IsGlobalNamespace)
            return topType;

        return SyntaxFactory.NamespaceDeclaration(
            SyntaxFactory.IdentifierName(sourceMemberSymbol.ContainingNamespace.ToDisplayString()),
            externs: default,
            usings: default,
            members: SyntaxFactory.SingletonList<MemberDeclarationSyntax>(topType));
    }

    #endregion

    [GeneratedCode("tool", "version")]
    public static AttributeListSyntax GeneratedCodeAttributeListSyntax(string tool, string version)
        => SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(
                    SyntaxFactory.IdentifierName($"{GlobalLiterals.Global_Keyword}::{GlobalLiterals.GeneratedCodeAttribute_TypeName}"),
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SeparatedList(new[] {
                            SyntaxFactory.AttributeArgument(
                                LiteralStringExpression(tool)),
                            SyntaxFactory.AttributeArgument(
                                LiteralStringExpression(version)),
                        })))));
}
