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
using Trarizon.Library.GeneratorToolkit.ContextModelExtensions;
using Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
using Trarizon.Library.GeneratorToolkit.More;
using static Trarizon.Library.SourceGeneration.Analyzers.FriendAccessLiterals;

namespace Trarizon.Library.SourceGeneration.Analyzers;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal partial class FriendAccessAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
        Diagnostic_FriendMemberCannotBeAccessed,
        Diagnostic_FriendMayBeAccessedByOtherAssembly,
        Diagnostic_FriendOnExplicitInterfaceMemberMakeNoSense,
        Diagnostic_SpecificTypeInTypeParameterMakeNoSense,
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSymbolAction(
            RequiresFriendNotPublicAccessable,
            SymbolKind.Method, SymbolKind.Field, SymbolKind.Property, SymbolKind.Event);

        context.RegisterOperationAction(
            CheckAccess,
            OperationKind.ObjectCreation, OperationKind.FieldReference, OperationKind.PropertyReference, OperationKind.MethodReference, OperationKind.EventReference);
    }

    private static void RequiresFriendNotPublicAccessable(SymbolAnalysisContext context)
    {
        var symbol = context.Symbol;

        if (symbol.IsImplicitlyDeclared)
            return;

        var friendAttr = symbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass.MatchDisplayString(L_Attribute_TypeName));
        if (friendAttr is null)
            return;

        CheckTypeParameter();
        if (!CheckExplicitInterfaceImplementation()) return;
        CheckPublicAccessibility();


        void CheckTypeParameter()
        {
            bool containsTypeArg = friendAttr.GetConstructorArrayArgument<ITypeSymbol>(L_Attribute_FriendTypes_ConstructorIndex)
                .OfType<INamedTypeSymbol>()
                .SelectMany(type => type.TypeArguments)
                .Any(typeArg => typeArg.TypeKind is not (TypeKind.TypeParameter or TypeKind.Error));

            if (containsTypeArg) {
                context.ReportDiagnostic(
                    Diagnostic_SpecificTypeInTypeParameterMakeNoSense,
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

        bool CheckExplicitInterfaceImplementation()
        {
            var isExplicit = symbol is
                IMethodSymbol { MethodKind: MethodKind.ExplicitInterfaceImplementation } or
                IPropertySymbol { ExplicitInterfaceImplementations.IsDefaultOrEmpty: false } or
                IEventSymbol { ExplicitInterfaceImplementations.IsDefaultOrEmpty: false };

            if (isExplicit) {
                context.ReportDiagnostic(
                    Diagnostic_FriendOnExplicitInterfaceMemberMakeNoSense,
                    // explicit method wont be partial
                    symbol.DeclaringSyntaxReferences[0].GetSyntax() switch
                    {
                        MethodDeclarationSyntax meth => meth.Identifier.GetLocation(),
                        PropertyDeclarationSyntax prop => prop.Identifier.GetLocation(),
                        EventDeclarationSyntax ev => ev.Identifier.GetLocation(),
                        var syntax => syntax.GetLocation(),
                    });
            }

            return !isExplicit;
        }

        void CheckPublicAccessibility()
        {
            // Ensure this member will not accessed by other assembly
            bool isInternal = TraEnumerable.EnumerateByNotNull(symbol, s => s.ContainingSymbol)
                .OfTypeUntil<ISymbol, INamespaceSymbol>()
                .Any(symbol => symbol.DeclaredAccessibility is
                    Accessibility.NotApplicable or
                    Accessibility.Internal or
                    Accessibility.Private or
                    Accessibility.ProtectedAndInternal); // private protected

            if (!isInternal) {
                // It's better to only warn on syntax that marked with attribute
                // but how to do that?
                context.ReportDiagnostic(
                    Diagnostic_FriendMayBeAccessedByOtherAssembly,
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
    }

    private static void CheckAccess(OperationAnalysisContext context)
    {
        var operation = context.Operation;

        if (!TryGetInterestedSymbol(out var memberSymbol, out var friendAttr))
            return;

        var friendTypes = friendAttr.GetConstructorArrayArgument<ITypeSymbol>(L_Attribute_FriendTypes_ConstructorIndex);
        var options = friendAttr.GetNamedArgument<FriendAccessOptions_Mirror>(L_Attribute_Options_PropertyIdentifier).Value;

        CheckAccess();


        bool TryGetInterestedSymbol([NotNullWhen(true)] out ISymbol? memberSymbol, [NotNullWhen(true)] out AttributeData? attribute)
        {
            attribute = null;
            memberSymbol = operation switch
            {
                IObjectCreationOperation objectCreation => objectCreation.Constructor,
                IMemberReferenceOperation memberReference => memberReference.Member,
                _ => null,
            };
            if (memberSymbol is null)
                return false;

            attribute = memberSymbol.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass.MatchDisplayString(L_Attribute_TypeName));
            if (attribute is not null)
                return true;

            // Maybe attribute marked on accessors
            IMethodSymbol? accessor = operation switch
            {
                IPropertyReferenceOperation propertyOperation => UsedPropertySetter(propertyOperation)
                    ? propertyOperation.Property.SetMethod
                    : propertyOperation.Property.GetMethod,
                IEventReferenceOperation
                {
                    Parent: ICompoundAssignmentOperation cAssign
                } eventOperation => cAssign.OperatorKind is BinaryOperatorKind.Add
                    ? eventOperation.Event.AddMethod
                    : eventOperation.Event.RemoveMethod,// Outside of the parent type, event will only appear on the left of += or -=
                _ => null,
            };

            attribute = accessor?.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass.MatchDisplayString(L_Attribute_TypeName));
            return attribute is not null;

            static bool UsedPropertySetter(IPropertyReferenceOperation propertyOperation)
            {
                var (target, parent) = propertyOperation.Ancestors(includeSelf: true)
                    .Adjacent()
                    .FirstOrDefault(tuple => tuple.Item2 is IAssignmentOperation);

                if (target is null)
                    return false;

                return target == ((IAssignmentOperation)parent).Target;
            }
        }

        void CheckAccess()
        {
            bool isFriend = operation.Syntax.Ancestors()
                .OfType<TypeDeclarationSyntax>()
                .Select(syntax => operation.SemanticModel.GetDeclaredSymbol(syntax))
                .OfNotNull()
                .CartesianProduct(friendTypes.Prepend(memberSymbol.ContainingType))
                .Any(CombinePredicates(options));
            if (isFriend)
                return;

            context.ReportDiagnostic(
                Diagnostic_FriendMemberCannotBeAccessed,
                operation.Syntax.GetLocation());

            Func<(INamedTypeSymbol, ITypeSymbol), bool> CombinePredicates(FriendAccessOptions_Mirror options)
            {
                return options.HasFlag(FriendAccessOptions_Mirror.AllowInherits)
                    ? AccessPredicate_AllowInherits
                    : AccessPredicate_None;
            }

            bool AccessPredicate_AllowInherits((INamedTypeSymbol, ITypeSymbol) tuple)
            {
                var (accessorType, friendType) = tuple;
                if (friendType.TypeKind is TypeKind.Interface) {
                    return accessorType.AllInterfaces
                        .AsEnumerable<ITypeSymbol>()
                        .Contains(friendType, MoreSymbolEqualityComparer.ByOriginalDefination);
                }
                else {
                    return TraEnumerable.EnumerateByNotNull(accessorType, t => t.BaseType)
                        .Contains(friendType, MoreSymbolEqualityComparer.ByOriginalDefination);
                }
            }
            bool AccessPredicate_None((INamedTypeSymbol, ITypeSymbol) tuple)
            {
                return MoreSymbolEqualityComparer.ByOriginalDefination.Equals(tuple.Item1, tuple.Item2);
            }
        }
    }
}
