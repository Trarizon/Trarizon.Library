using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.Library.GeneratorToolkit.Extensions;

namespace Trarizon.Library.SourceGenerator.Analyzers;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal partial class FriendAccessAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Literals.Diagnostic_FriendMemberCannotBeAccessed,
        Literals.Diagnostic_FriendMayBeAccessedByOtherAssembly);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(
            FriendShouldBeInternal,
            SyntaxKind.MethodDeclaration, SyntaxKind.FieldDeclaration, SyntaxKind.PropertyDeclaration, SyntaxKind.EventDeclaration, SyntaxKind.ConstructorDeclaration);
        
        context.RegisterSyntaxNodeAction(
            CheckIfCalledFromFriendOrSelf,
            SyntaxKind.SimpleMemberAccessExpression); // Removed IdentifierName, directly access in type is always allow
    }

    private static void FriendShouldBeInternal(SyntaxNodeAnalysisContext context)
    {
        var syntax = (MemberDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(syntax is FieldDeclarationSyntax field ? field.Declaration.Variables[0] : syntax);

        if (true != symbol?.GetAttributes().Any(attr => attr.AttributeClass.MatchDisplayString(Literals.FriendAttribute_TypeName)))
            return;

        // One of ancestors' modifier should be internal
        bool isInternal = syntax.AncestorsAndSelf().OfType<MemberDeclarationSyntax>().Any(syntax =>
        {
            // internal
            // internal protected (x
            // private 
            // private protected
            const int Internal = 1, Protected = 2;
            int mod = 0;
            foreach (var modifier in syntax.Modifiers) {
                if (modifier.IsKind(SyntaxKind.InternalKeyword)) {
                    if (mod == Protected) // internal protected
                        return false;
                    mod = Internal;
                }
                else if (modifier.IsKind(SyntaxKind.ProtectedKeyword)) {
                    if (mod == Internal) // internal protected
                        return false;
                    mod = Protected;
                }
                else if (modifier.IsKind(SyntaxKind.PrivateKeyword))
                    return true; // private (protected)
                else if (modifier.IsKind(SyntaxKind.PublicKeyword))
                    return false; // public
            }
            return mod switch {
                Internal => true,
                Protected => false,
                _ => true, // no access modifier
            };
        });
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
        var memberAccessExprSyntax = (ExpressionSyntax)context.Node;
        var memberSymbol = context.SemanticModel.GetSymbolInfo(memberAccessExprSyntax).Symbol;

        var friendAttr = memberSymbol?.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass.MatchDisplayString(Literals.FriendAttribute_TypeName));
        if (friendAttr is null)
            return;

        var friendTypes = friendAttr.GetConstructorArguments<ITypeSymbol>(Literals.FriendAttribute_FriendTypes_ConstructorIndex);

        bool isFriend = memberAccessExprSyntax.Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .Select(syntax => context.SemanticModel.GetDeclaredSymbol(syntax)!)
            .CartesianProduct(friendTypes.Prepend(memberSymbol!.ContainingType))
            .Any(tuple => SymbolEqualityComparer.Default.Equals(tuple.Item1.OriginalDefinition, tuple.Item2.OriginalDefinition));
        if (isFriend)
            return;

        context.ReportDiagnostic(
            Literals.Diagnostic_FriendMemberCannotBeAccessed,
            memberAccessExprSyntax switch {
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
                IdentifierNameSyntax identifierName => identifierName,
                _ => memberAccessExprSyntax,
            });
    }
}
