using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.Library.Collections;
using Trarizon.Library.Roslyn.Extensions;

namespace Trarizon.Library.Generators.CodeAnalysis;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal partial class FriendAccessAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
        Diag.FriendMemberCannotBeAccessed,
        Diag.FriendOnExplicitInterfaceMemberMakeNoSense,
        Diag.FriendMayBeAccessedByOtherAssembly,
        Diag.SpecificTypeInTypeParameterMakeNoSense,
        ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSymbolAction(
            RequiresFriendNonPublicAccessable,
            SymbolKind.Method, SymbolKind.Field, SymbolKind.Property, SymbolKind.Event);

        context.RegisterOperationAction(
            CheckAccess,
            OperationKind.ObjectCreation, OperationKind.FieldReference, OperationKind.PropertyReference, OperationKind.MethodReference, OperationKind.EventReference);
    }

    private static void RequiresFriendNonPublicAccessable(SymbolAnalysisContext context)
    {
        var symbol = context.Symbol;
        if (symbol.IsImplicitlyDeclared)
            return;

        var friendAttrData = symbol.GetAttributes()
            .FirstOrDefault(AttributeProxy.IsThisType);
        if (friendAttrData is null)
            return;
        var friendAttr = new AttributeProxy(friendAttrData);

        CheckTypeParameter();
        if (!CheckExplicitInterfaceImplementation())
            return;
        CheckPublicAccessbility();

        return;

        void CheckTypeParameter()
        {
            bool containsTypeArg = friendAttr.FriendTypes
                .OfType<INamedTypeSymbol>()
                .SelectMany(type => type.TypeArguments)
                .Any(arg => arg.TypeKind is not (TypeKind.TypeParameter or TypeKind.Error));

            if (!containsTypeArg)
                return;

            context.ReportDiagnostic(
                Diag.SpecificTypeInTypeParameterMakeNoSense,
                symbol.DeclaringSyntaxReferences[0].GetSyntax() switch
                {
                    MethodDeclarationSyntax met => met.Identifier.GetLocation(),
                    FieldDeclarationSyntax fie => fie.Declaration.GetLocation(),
                    PropertyDeclarationSyntax prop => prop.Identifier.GetLocation(),
                    EventDeclarationSyntax ev => ev.Identifier.GetLocation(),
                    ConstructorDeclarationSyntax ctor => ctor.Identifier.GetLocation(),
                    AccessorDeclarationSyntax accessor => accessor.Keyword.GetLocation(),
                    var syntax => syntax.GetLocation(),
                });
        }

        bool CheckExplicitInterfaceImplementation()
        {
            bool isExplicit = symbol is
                IMethodSymbol { MethodKind: MethodKind.ExplicitInterfaceImplementation } or
                IPropertySymbol { ExplicitInterfaceImplementations.IsDefaultOrEmpty: false } or
                IEventSymbol { ExplicitInterfaceImplementations.IsDefaultOrEmpty: false };

            if (!isExplicit)
                return true;

            context.ReportDiagnostic(
                Diag.FriendOnExplicitInterfaceMemberMakeNoSense,
                symbol.DeclaringSyntaxReferences[0].GetSyntax() switch
                {
                    MethodDeclarationSyntax met => met.Identifier.GetLocation(),
                    PropertyDeclarationSyntax prop => prop.Identifier.GetLocation(),
                    EventDeclarationSyntax ev => ev.Identifier.GetLocation(),
                    var syntax => syntax.GetLocation(),
                });
            return false;
        }

        void CheckPublicAccessbility()
        {
            // To ensure this member will not accessed by other assembly

            bool isInternal = symbol.ContainingSymbols()
                .OfTypeUntil<ISymbol, INamespaceSymbol>()
                .Any(symbol => symbol.DeclaredAccessibility is
                    Accessibility.NotApplicable or
                    Accessibility.Internal or
                    Accessibility.Private or
                    Accessibility.ProtectedAndInternal); // private protected

            if (isInternal)
                return;

            context.ReportDiagnostic(
                Diag.FriendMayBeAccessedByOtherAssembly,
                symbol.DeclaringSyntaxReferences[0].GetSyntax() switch
                {
                    MethodDeclarationSyntax meth => meth.Identifier.GetLocation(),
                    FieldDeclarationSyntax fie => fie.Declaration.GetLocation(),
                    PropertyDeclarationSyntax prop => prop.Identifier.GetLocation(),
                    EventDeclarationSyntax ev => ev.Identifier.GetLocation(),
                    ConstructorDeclarationSyntax ctor => ctor.Identifier.GetLocation(),
                    AccessorDeclarationSyntax accessor => accessor.Keyword.GetLocation(),
                    var syntax => syntax.GetLocation(),
                });
        }
    }

    private static void CheckAccess(OperationAnalysisContext context)
    {
        var operation = context.Operation;

        if (!TryGetInterestedSymbol(out var memberSymbol, out var attribute))
            return;

        var friendTypes = attribute.FriendTypes;
        var options = attribute.Options;

        // Ready.

        bool isFriend = operation.Syntax.Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .Select(syntax => operation.SemanticModel.GetDeclaredSymbol(syntax))
            .OfNotNull()
            .CartesianProduct(friendTypes.Prepend(memberSymbol.ContainingType))
            .Any(options.HasFlag(FriendAccessOptionsMirror.AllowInherits)
                ? AccessPredicate_AllowInherits
                : AccessPredicate_None);

        if (isFriend)
            return;

        context.ReportDiagnostic(
            Diag.FriendMemberCannotBeAccessed,
            operation.Syntax.GetLocation());

        return;

        bool TryGetInterestedSymbol([NotNullWhen(true)] out ISymbol? memberSymbol, out AttributeProxy attributeProxy)
        {
            AttributeData? attribute = null;
            attributeProxy = default;
            memberSymbol = operation switch
            {
                IObjectCreationOperation objCreat => objCreat.Constructor,
                IMemberReferenceOperation memberRef => memberRef.Member,
                _ => null,
            };
            if (memberSymbol is null)
                return false;

            attribute = memberSymbol.GetAttributes()
                .FirstOrDefault(AttributeProxy.IsThisType);

            if (attribute is not null) {
                attributeProxy = new(attribute);
                return true;
            }

            // Attribute may be marked on accessors
            IMethodSymbol? accessor = operation switch
            {
                IPropertyReferenceOperation prop => IsAccessingPropertySetter(prop)
                    ? prop.Property.SetMethod
                    : prop.Property.GetMethod,
                IEventReferenceOperation { Parent: ICompoundAssignmentOperation cassign } ev => cassign.OperatorKind is BinaryOperatorKind.Add
                    ? ev.Event.AddMethod
                    : ev.Event.RemoveMethod, // Outside of the parent type, event will only appear on the left of += or -=
                _ => null,
            };

            attribute = accessor?.GetAttributes()
                .FirstOrDefault(AttributeProxy.IsThisType);
            if (attribute is not null) {
                attributeProxy = new(attribute);
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

        bool AccessPredicate_AllowInherits((INamedTypeSymbol, ITypeSymbol) tpl)
        {
            var (accessor, friend) = tpl;
            if (friend.TypeKind is TypeKind.Interface) {
                return accessor.AllInterfaces
                    .AsEnumerable()
                    .Contains(friend, SymbolExt.OriginalDefinationEqualityComparer);
            }
            else {
                return TraEnumerable.EnumerateByNotNull(accessor, t => t.BaseType)
                    .Contains(friend, SymbolExt.OriginalDefinationEqualityComparer);
            }
        }

        bool AccessPredicate_None((INamedTypeSymbol, ITypeSymbol) tpl)
            => SymbolExt.OriginalDefinationEqualityComparer.Equals(tpl.Item1, tpl.Item2);
    }
}
