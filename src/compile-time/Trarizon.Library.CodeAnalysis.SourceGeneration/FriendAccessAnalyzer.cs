using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.Library.Collections;
using Trarizon.Library.Roslyn.Diagnostics;
using Trarizon.Library.Roslyn.Extensions;

namespace Trarizon.Library.CodeAnalysis.SourceGeneration;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed partial class FriendAccessAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = Descriptors.GetDescriptors();

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSymbolAction(
            CheckDefination,
            SymbolKind.Method, SymbolKind.Field, SymbolKind.Property, SymbolKind.Event);

        context.RegisterOperationAction(
            CheckAccesses,
            OperationKind.ObjectCreation,
            OperationKind.FieldReference,
            OperationKind.PropertyReference,
            OperationKind.MethodReference,
            OperationKind.EventReference,
            OperationKind.Invocation);
    }

    private static void CheckDefination(SymbolAnalysisContext context)
    {
        var symbol = context.Symbol;
        if (symbol.IsImplicitlyDeclared)
            return;

        var friendAttrData = symbol.GetAttributes()
            .FirstOrDefault(RuntimeAttribute.IsThisType);

        if (friendAttrData is null)
            return;
        var friendAttr = new RuntimeAttribute(friendAttrData);

        context.ReportDiagnosticWhenNotNull(CheckGenericTypeUnbounded());
        if (context.ReportDiagnosticWhenNotNull(CheckExplicitInterfaceImplementation()))
            return;
        context.ReportDiagnosticWhenNotNull(CheckAccessibility());

        return;

        DiagnosticData? CheckGenericTypeUnbounded()
        {
            // Generic type should be unbounded
            bool containsTypeArg = friendAttr.GetFriendTypes()
                .OfType<INamedTypeSymbol>()
                .SelectMany(type => type.TypeArguments)
                .Any(arg => arg.TypeKind is not (TypeKind.TypeParameter or TypeKind.Error));
            if (!containsTypeArg)
                return null;

            return new DiagnosticData(
                Descriptors.FriendTypeRecommendBeUnbounded,
                symbol.DeclaringSyntaxReferences[0].GetSyntax() switch
                {
                    MethodDeclarationSyntax met => met.Identifier.GetLocation(),
                    FieldDeclarationSyntax fie => fie.Declaration.GetLocation(),
                    PropertyDeclarationSyntax pro => pro.Identifier.GetLocation(),
                    EventDeclarationSyntax eve => eve.Identifier.GetLocation(),
                    ConstructorDeclarationSyntax con => con.Identifier.GetLocation(),
                    AccessorDeclarationSyntax acc => acc.Keyword.GetLocation(),
                    var syn => syn.GetLocation(),
                });
        }

        DiagnosticData? CheckExplicitInterfaceImplementation()
        {
            bool isExplicit = symbol is
                IMethodSymbol { MethodKind: MethodKind.ExplicitInterfaceImplementation } or
                IPropertySymbol { ExplicitInterfaceImplementations.IsDefaultOrEmpty: false } or
                IEventSymbol { ExplicitInterfaceImplementations.IsDefaultOrEmpty: false };

            if (!isExplicit)
                return null;

            return new DiagnosticData(
                Descriptors.FriendTypeShouldNotOnExplicitInterfaceMember,
                symbol.DeclaringSyntaxReferences[0].GetSyntax() switch
                {
                    MethodDeclarationSyntax met => met.Identifier.GetLocation(),
                    PropertyDeclarationSyntax prop => prop.Identifier.GetLocation(),
                    EventDeclarationSyntax ev => ev.Identifier.GetLocation(),
                    var syntax => syntax.GetLocation(),
                });
        }

        DiagnosticData? CheckAccessibility()
        {
            // To ensure this member will not accessed by other assembly

            bool isNonPublic = symbol.ContainingSymbols()
                .TakeWhile(s => s is not INamespaceSymbol)
                .Any(s => s.DeclaredAccessibility is
                    Accessibility.NotApplicable or
                    Accessibility.Internal or
                    Accessibility.Private or
                    Accessibility.ProtectedAndInternal);

            if (isNonPublic)
                return null;

            return new DiagnosticData(
                Descriptors.FriendMemberMayBeAccessedByOtherAssembly,
                symbol.DeclaringSyntaxReferences[0].GetSyntax() switch
                {
                    MethodDeclarationSyntax met => met.Identifier.GetLocation(),
                    FieldDeclarationSyntax fie => fie.Declaration.GetLocation(),
                    PropertyDeclarationSyntax pro => pro.Identifier.GetLocation(),
                    EventDeclarationSyntax eve => eve.Identifier.GetLocation(),
                    ConstructorDeclarationSyntax con => con.Identifier.GetLocation(),
                    AccessorDeclarationSyntax acc => acc.Keyword.GetLocation(),
                    var syn => syn.GetLocation(),
                });
        }
    }

    private static void CheckAccesses(OperationAnalysisContext context)
    {
        var operation = context.Operation;
        Debug.WriteLine(operation);
        if (!TryGetInterestedSymbol(out var memberSymbol, out var attribute))
            return;

        var friendTypes = attribute.GetFriendTypes();

        // Ready.

        bool isFriend = operation.Syntax.Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .Select(syntax => operation.SemanticModel.GetDeclaredSymbol(syntax))
            .OfNotNull()
            .CartesianProduct(friendTypes.Prepend(memberSymbol.ContainingType))
            .Any(tpl => SymbolEqualityComparer.OriginalDefination.Equals(tpl.Item1, tpl.Item2));

        if (isFriend)
            return;

        context.ReportDiagnostic(
            Descriptors.FriendMemberCannotBeAccessed,
            operation.Syntax.GetLocation());

        return;

        bool TryGetInterestedSymbol([MaybeNullWhen(false)] out ISymbol memberSymbol, out RuntimeAttribute attribute)
        {
            attribute = default;
            memberSymbol = operation switch
            {
                IObjectCreationOperation objCreat => objCreat.Constructor,
                IMemberReferenceOperation memberRef => memberRef.Member,
                IInvocationOperation inv => inv.TargetMethod,
                _ => null,
            };

            if (memberSymbol is null)
                return false;

            var attr = memberSymbol.GetAttributes()
                 .FirstOrDefault(RuntimeAttribute.IsThisType);

            if (attr is not null) {
                attribute = new(attr);
                return true;
            }

            // Attribute on property/event accessors
            IMethodSymbol? accessor = operation switch
            {
                IPropertyReferenceOperation prop => IsAccessingPropertySetter(prop)
                    ? prop.Property.SetMethod : prop.Property.GetMethod,
                IEventReferenceOperation { Parent: ICompoundAssignmentOperation cass } ev => cass.OperatorKind is BinaryOperatorKind.Add
                    ? ev.Event.AddMethod : ev.Event.RemoveMethod, // Outside of the parent type, event will only appear on the left of += or -=
                _ => null,
            };

            attr = accessor?.GetAttributes()
                .FirstOrDefault(RuntimeAttribute.IsThisType);
            if (attr is not null) {
                attribute = new RuntimeAttribute(attr);
                return true;
            }

            return false;

            static bool IsAccessingPropertySetter(IPropertyReferenceOperation propRef)
            {
                var (target, parent) = propRef.Ancestors(includeSelf: true)
                    .Adjacent()
                    .FirstOrDefault(tpl => tpl.Item2 is IAssignmentOperation);
                if (target is null)
                    return false;

                return target == ((IAssignmentOperation)parent).Target;
            }
        }
    }
}
