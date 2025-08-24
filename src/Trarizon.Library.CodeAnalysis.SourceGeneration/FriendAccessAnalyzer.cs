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
using Trarizon.Library.Roslyn.Extensions;

namespace Trarizon.Library.CodeAnalysis.SourceGeneration;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed partial class FriendAccessAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
        Descriptors.FriendMemberCannotBeAccessed,
        Descriptors.FriendMemberMayBeAccessedByOtherAssembly,
        Descriptors.FriendTypeShouldNotOnExplicitInterfaceMember,
        Descriptors.FriendTypeRecommendBeUnbounded,
        ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(context =>
        {
            var compilation = context.Compilation;
            if (!compilation.TryGetFriendAccessAttribute(out var friendAccessAttributeSymbol))
                return;

            // Check defination
            context.RegisterSymbolAction(context =>
            {
                var symbol = context.Symbol;
                if (symbol.IsImplicitlyDeclared)
                    return;

                if (!symbol.TryGetAttributeData(friendAccessAttributeSymbol, out var friendAttr))
                    return;

                // Generic type should be unbounded
                if (!IsGenericTypeUnbounded()) {
                    context.ReportDiagnostic(Descriptors.FriendTypeRecommendBeUnbounded,
                        symbol.Locations[0]);
                }
                // No use on explicit interface
                if (IsExplicitInterfaceImplementation()) {
                    context.ReportDiagnostic(Descriptors.FriendTypeShouldNotOnExplicitInterfaceMember,
                        symbol.Locations[0]);
                    return;
                }
                if (IsPublicAccess()) {
                    context.ReportDiagnostic(Descriptors.FriendMemberMayBeAccessedByOtherAssembly,
                        symbol.Locations[0]);
                }
                return;

                bool IsGenericTypeUnbounded()
                {
                    var friendTypes = friendAttr.GetConstructorArgument(0).CastArray<ITypeSymbol>();

                    bool containsTypeArg = friendTypes
                        .OfType<INamedTypeSymbol>()
                        .SelectMany(type => type.TypeArguments)
                        .Any(arg => arg.TypeKind is not (TypeKind.TypeParameter or TypeKind.Error));

                    return !containsTypeArg;
                }

                bool IsExplicitInterfaceImplementation()
                {
                    bool isExplicit = symbol is
                        IMethodSymbol { MethodKind: MethodKind.ExplicitInterfaceImplementation } or
                        IPropertySymbol { ExplicitInterfaceImplementations.IsDefaultOrEmpty: false } or
                        IEventSymbol { ExplicitInterfaceImplementations.IsDefaultOrEmpty: false };

                    return isExplicit;
                }

                bool IsPublicAccess()
                {
                    // To ensure this member will not accessed by other assembly

                    bool isNonPublic = symbol.ContainingSymbols()
                        .TakeWhile(s => s is not INamespaceSymbol)
                        .Any(s => s.DeclaredAccessibility is
                            Accessibility.NotApplicable or
                            Accessibility.Internal or
                            Accessibility.Private or
                            Accessibility.ProtectedAndInternal);

                    return !isNonPublic;
                }
            }, SymbolKind.Method, SymbolKind.Field, SymbolKind.Property, SymbolKind.Event);

            // Check access
            context.RegisterOperationAction(context =>
            {
                var operation = context.Operation;
                Debug.WriteLine(operation);
                if (!TryGetInterestedSymbol(out var memberSymbol, out var attribute))
                    return;

                var friendTypes = attribute.GetConstructorArgument(0).CastArray<ITypeSymbol>();

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

                bool TryGetInterestedSymbol([MaybeNullWhen(false)] out ISymbol memberSymbol, [MaybeNullWhen(false)] out AttributeData attribute)
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

                    if (memberSymbol.TryGetAttributeData(friendAccessAttributeSymbol, out var attr)) {
                        attribute = attr;
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

                    if (accessor?.TryGetAttributeData(friendAccessAttributeSymbol, out var accessorAttr) ?? false) {
                        attribute = accessorAttr;
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
            },
            OperationKind.ObjectCreation,
            OperationKind.FieldReference,
            OperationKind.PropertyReference,
            OperationKind.MethodReference,
            OperationKind.EventReference,
            OperationKind.Invocation);
        });
    }
}
