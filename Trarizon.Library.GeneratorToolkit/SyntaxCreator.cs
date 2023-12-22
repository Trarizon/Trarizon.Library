using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using Trarizon.Library.GeneratorToolkit.Extensions;

namespace Trarizon.Library.GeneratorToolkit;
/// <summary>
/// Extensions of <see cref="SyntaxFactory"/>,
/// </summary>
public static class SyntaxCreator
{
    public static ParameterListSyntax EmptyParameterListSyntax = SyntaxFactory.ParameterList();
    public static ArgumentListSyntax EmptyArgumentListSyntax = SyntaxFactory.ArgumentList();

    public static SyntaxToken SemicolonToken = SyntaxFactory.Token(SyntaxKind.SemicolonToken);

    public static SyntaxTokenList Modifiers(params SyntaxKind[] modifiers)
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
    public static TypeDeclarationSyntax ClonePartialDeclaration(TypeDeclarationSyntax self,
        SyntaxList<AttributeListSyntax> attributeLists,
        BaseListSyntax? baseList,
        SyntaxList<MemberDeclarationSyntax> members)
        => self switch {
            ClassDeclarationSyntax clz => ClonePartialDeclaration(clz, attributeLists, baseList, members),
            StructDeclarationSyntax str => ClonePartialDeclaration(str, attributeLists, baseList, members),
            RecordDeclarationSyntax rec => ClonePartialDeclaration(rec, attributeLists, baseList, members),
            InterfaceDeclarationSyntax itf => ClonePartialDeclaration(itf, attributeLists, baseList, members),
            _ => throw new InvalidOperationException("Unknown type declaration"),
        };

    /// <summary>
    /// Create a partial class by copy the basic info of original class
    /// </summary>
    public static ClassDeclarationSyntax ClonePartialDeclaration(ClassDeclarationSyntax self,
        SyntaxList<AttributeListSyntax> attributeLists,
        BaseListSyntax? baseList,
        SyntaxList<MemberDeclarationSyntax> members)
        => SyntaxFactory.ClassDeclaration(
            attributeLists,
            Modifiers(SyntaxKind.PartialKeyword),
            self.Identifier,
            self.TypeParameterList,
            baseList,
            default,
            members);

    /// <summary>
    /// Create a partial struct by copy the basic info of original struct
    /// </summary>
    public static StructDeclarationSyntax ClonePartialDeclaration(StructDeclarationSyntax self,
        SyntaxList<AttributeListSyntax> attributeLists,
        BaseListSyntax? baseList,
        SyntaxList<MemberDeclarationSyntax> members)
        => SyntaxFactory.StructDeclaration(
            attributeLists,
            Modifiers(SyntaxKind.PartialKeyword),
            self.Identifier,
            self.TypeParameterList,
            baseList,
            default,
            members);

    /// <summary>
    /// Create a partial interface by copy the basic info of original interface
    /// </summary>
    public static InterfaceDeclarationSyntax ClonePartialDeclaration(InterfaceDeclarationSyntax self,
        SyntaxList<AttributeListSyntax> attributeLists,
        BaseListSyntax? baseList,
        SyntaxList<MemberDeclarationSyntax> members)
        => SyntaxFactory.InterfaceDeclaration(
            attributeLists,
            Modifiers(SyntaxKind.PartialKeyword),
            self.Identifier,
            self.TypeParameterList,
            baseList,
            default,
            members);

    /// <summary>
    /// Create a partial record by copy the basic info of original record
    /// </summary>
    public static RecordDeclarationSyntax ClonePartialDeclaration(RecordDeclarationSyntax self,
        SyntaxList<AttributeListSyntax> attributeLists,
        BaseListSyntax? baseList,
        SyntaxList<MemberDeclarationSyntax> members)
        => SyntaxFactory.RecordDeclaration(
            attributeLists,
            Modifiers(SyntaxKind.PartialKeyword),
            SyntaxFactory.Token(SyntaxKind.RecordKeyword),
            self.Identifier,
            self.TypeParameterList,
            default,
            baseList,
            default,
            members);

    #endregion

    /// <summary>
    /// Create all containing partial types of a type by copy the basic info of original types
    /// </summary>
    public static TypeDeclarationSyntax CloneAllContainingTypeDeclarations(TypeDeclarationSyntax nestedType,
        IAttributeSyntaxContext<TypeDeclarationSyntax, ITypeSymbol> contextUtil)
    {
        var (syntax, symbol) = (contextUtil.Syntax, contextUtil.Symbol);
        var type = nestedType;

        while (symbol.ContainingType != null) {
            symbol = symbol.ContainingType;
            syntax = (TypeDeclarationSyntax)syntax.Parent!;
            type = ClonePartialDeclaration(syntax, default, default,
                SyntaxFactory.List<MemberDeclarationSyntax>([type]));
        }

        return type;
    }

    /// <summary>
    /// Create namespace by clone given ITypeSymbol 
    /// </summary>
    public static MemberDeclarationSyntax CloneNamespaceDeclaration(TypeDeclarationSyntax type,
        ITypeSymbol typeUtil)
    {
        string nsString = typeUtil.ContainingNamespace.ToDisplayString();
        if (nsString == Literals.GlobalNamespaceDisplayString)
            return type;

        return SyntaxFactory.NamespaceDeclaration(
            SyntaxFactory.ParseName(nsString),
            externs: default,
            usings: default,
            members: SyntaxFactory.List<MemberDeclarationSyntax>([type]));
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
