using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.Library.SourceGenerator.Toolkit;
using Trarizon.Library.SourceGenerator.Toolkit.Extensions;

namespace Trarizon.Library.SourceGenerator.Analyzers;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal partial class FriendAccessAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Literals.Diagnostic_FriendMemberCannotBeAccessed,
        Literals.Diagnostic_FriendMayBeAccessedByOtherAssembly,
        Literals.Diagnostic_FriendOnExplicitInterfaceMemberMakeNoSense);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(
            FriendShouldBeInternal,
            SyntaxKind.MethodDeclaration, SyntaxKind.FieldDeclaration, SyntaxKind.PropertyDeclaration, SyntaxKind.EventDeclaration, SyntaxKind.ConstructorDeclaration);

        context.RegisterSyntaxNodeAction(
            CheckIfCalledFromFriendOrSelf,
            SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.PointerMemberAccessExpression, SyntaxKind.ObjectCreationExpression, SyntaxKind.ImplicitObjectCreationExpression); // Removed IdentifierName, directly access in type is always allow
    }

    private static void FriendShouldBeInternal(SyntaxNodeAnalysisContext context)
    {
        var syntax = (MemberDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(syntax is FieldDeclarationSyntax field ? field.Declaration.Variables[0] : syntax);

        if (true != symbol?.GetAttributes().Any(attr => attr.AttributeClass.MatchDisplayString(Literals.Attribute_TypeName)))
            return;

        // Not explicit interface member
        if (syntax is BasePropertyDeclarationSyntax { ExplicitInterfaceSpecifier: { } }
            or MethodDeclarationSyntax { ExplicitInterfaceSpecifier: { } }
        ) {
            context.ReportDiagnostic(
                Literals.Diagnostic_FriendOnExplicitInterfaceMemberMakeNoSense,
                syntax switch {
                    MethodDeclarationSyntax meth => meth.Identifier,
                    PropertyDeclarationSyntax prop => prop.Identifier,
                    EventDeclarationSyntax ev => ev.Identifier,
                    _ => syntax,
                });
            return;
        }

        // Ensure this member will not accessed by other assembly
        bool isInternal = syntax.AncestorsAndSelf()
            .OfTypeUntil<MemberDeclarationSyntax, BaseNamespaceDeclarationSyntax>()
            .Any(syntax => syntax.GetAccessModifiers() is
                AccessModifiers.None or
                AccessModifiers.Internal or
                AccessModifiers.Private or
                AccessModifiers.PrivateProtected);

        if (isInternal)
            return;

        context.ReportDiagnostic(
            Literals.Diagnostic_FriendMayBeAccessedByOtherAssembly,
            syntax switch {
                MethodDeclarationSyntax meth => meth.Identifier,
                FieldDeclarationSyntax fie => fie.Declaration,
                PropertyDeclarationSyntax prop => prop.Identifier,
                EventDeclarationSyntax ev => ev.Identifier,
                ConstructorDeclarationSyntax ctor => ctor.Identifier,
                _ => syntax,
            });
    }

    private static void CheckIfCalledFromFriendOrSelf(SyntaxNodeAnalysisContext context)
    {
        // MemberAccessExpr
        // ObjectCreationExpr
        // ImplicitObjectCreationExpr
        var accessExprSyntax = (ExpressionSyntax)context.Node;
        var memberSymbol = context.SemanticModel.GetSymbolInfo(accessExprSyntax).Symbol;

        var friendAttr = memberSymbol?.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass.MatchDisplayString(Literals.Attribute_TypeName));
        if (friendAttr is null)
            return;

        var friendTypes = friendAttr.GetConstructorArguments<ITypeSymbol>(Literals.Attribute_FriendTypes_ConstructorIndex);

        bool isFriend = accessExprSyntax.Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .Select(syntax => context.SemanticModel.GetDeclaredSymbol(syntax)!)
            .CartesianProduct(friendTypes.Prepend(memberSymbol!.ContainingType))
            .Any(tuple => SymbolEqualityComparer.Default.Equals(tuple.Item1.OriginalDefinition, tuple.Item2.OriginalDefinition));
        if (isFriend)
            return;

        context.ReportDiagnostic(
            Literals.Diagnostic_FriendMemberCannotBeAccessed,
            accessExprSyntax switch {
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
                BaseObjectCreationExpressionSyntax => accessExprSyntax,
                _ => accessExprSyntax,
            });
    }
}
