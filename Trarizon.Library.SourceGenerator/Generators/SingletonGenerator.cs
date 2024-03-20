using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.Library.SourceGenerator.Toolkit;
using Trarizon.Library.SourceGenerator.Toolkit.Extensions;
using Trarizon.Library.SourceGenerator.Toolkit.Factories;

namespace Trarizon.Library.SourceGenerator.Generators;
[Generator(LanguageNames.CSharp)]
internal sealed partial class SingletonGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var filter = context.SyntaxProvider.ForAttributeWithMetadataName(
             Literals.Attribute_TypeName,
             (node, token) => true, // class or record
             (context, token) =>
             {
                 var diagContext = new DiagnosticContext<ValidationContext>(new ValidationContext(context))
                    .Validate(v => v.ValidateClassDeclaration())
                    .Validate(v => v.ValidateIsSealed())
                    .Validate(v => v.ValidateHasSinglePrivateNonParamCtor());
                 return diagContext;
             });

        context.RegisterSourceOutput(filter, (context, diagContext) =>
        {
            foreach (var diag in diagContext.Diagnostics) {
                context.ReportDiagnostic(diag);
            }
            if (diagContext.Value.IsGeneratable) {
                context.AddSource(
                    diagContext.Value.TypeSymbol.ToCsFileNameString(Constants.SingletonGenerator_Suffix),
                    new GenerationContext(diagContext.Value).GenerateCompilationUnit());
            }
        });
    }

    private class ValidationContext(GeneratorAttributeSyntaxContext context)
    {
        [MemberNotNullWhen(true, nameof(_typeSyntax), nameof(TypeSyntax))]
        public bool IsGeneratable { get; private set; } = true;

        private ClassDeclarationSyntax? _typeSyntax;
        public ClassDeclarationSyntax TypeSyntax
        {
            get => _typeSyntax!;
            private set => _typeSyntax = value;
        }

        private ITypeSymbol? _typeSymbol;
        public ITypeSymbol TypeSymbol => _typeSymbol ??= context.SemanticModel.GetDeclaredSymbol(TypeSyntax)!;

        public bool HasPrivateCtor { get; private set; }

        #region AttributeDatas

        private AttributeData Attribute => context.Attributes[0];

        private Optional<string?> _instancePropertyIdentifier;
        public string? InstancePropertyIdentifier
        {
            get {
                if (!_instancePropertyIdentifier.HasValue) {
                    _instancePropertyIdentifier = Attribute.GetNamedArgument<string>(Literals.Attribute_InstancePropertyName_PropertyIdentifier);
                }
                return _instancePropertyIdentifier.Value;
            }
        }

        private Optional<string?> _singletonProviderIdentifier;
        public string? SingletonProviderIdentifier
        {
            get {
                if (!_singletonProviderIdentifier.HasValue) {
                    _singletonProviderIdentifier = Attribute.GetNamedArgument<string>(Literals.Attribute_SingletonProviderName_PropertyIdentifier);
                }
                return _singletonProviderIdentifier.Value;
            }
        }

        public SingletonOptions Options => Attribute.GetNamedArgument<SingletonOptions>(Literals.Attribute_Options_PropertyIdentifier);

        [Flags]
        public enum SingletonOptions
        {
            None = 0,
            /// <summary>
            /// Do not use provider to create instance
            /// </summary>
            /// <remarks>
            /// By default, we create a nested class with a public field as provider <br/>
            /// If this option set, we directly create and assign to instance property, 
            /// in which case when you use this instance, all other static fields in this
            /// type will be initialized.
            /// </remarks>
            NoProvider = 1 << 0,
            /// <summary>
            /// Mark Instance property internal
            /// </summary>
            IsInternalInstance = 1 << 1,
        }

        #endregion

        public Diagnostic? ValidateClassDeclaration()
        {
            var targetNode = (TypeDeclarationSyntax)context.TargetNode;
            if (context.TargetNode is ClassDeclarationSyntax typeSyntax) {
                TypeSyntax = typeSyntax;
                return null;
            }

            IsGeneratable = false;
            return DiagnosticFactory.Create(
                Literals.Diagnostic_SingletonIsClassOnly,
                targetNode.Identifier);
        }

        public Diagnostic? ValidateIsSealed()
        {
            if (!IsGeneratable)
                return null;

            if (TypeSyntax.Modifiers.Any(SyntaxKind.SealedKeyword))
                return null;

            return DiagnosticFactory.Create(
                Literals.Diagnostic_SingletonIsSealed,
                TypeSyntax.Identifier);
        }

        public Diagnostic? ValidateHasSinglePrivateNonParamCtor()
        {
            if (!IsGeneratable)
                return null;

            // No primary constructor // primary constructor is never private
            if (TypeSyntax.ParameterList is not null)
                goto ReportDiagnostic;

            var ctorOpt = TypeSyntax.ChildNodes()
                 .OfType<ConstructorDeclarationSyntax>()
                 // Exclude static ctors
                 .Where(ctor => !ctor.Modifiers.Any(SyntaxKind.StaticKeyword))
                 .TrySingle();

            switch (ctorOpt.ResultKind) {
                case EnumerableExtensions.SingleOptionalKind.Empty:
                    HasPrivateCtor = false;
                    return null;
                case EnumerableExtensions.SingleOptionalKind.Single:
                    break;
                default: //multiple
                    goto ReportDiagnostic;
            }

            var ctor = ctorOpt.GetValueOrDefault()!;
            if (ctor.GetAccessModifiers() is not AccessModifiers.None and not AccessModifiers.Private) {
                // Is non-private
                goto ReportDiagnostic;
            }
            HasPrivateCtor = true;

            // Is not non-param ctor
            if (ctor.ParameterList.Parameters.Count > 0) {
                return DiagnosticFactory.Create(
                    Literals.Diagnostic_SingletonCtorHasNoParameter,
                    ctor.Identifier);
            }

            return null;

        ReportDiagnostic:
            return DiagnosticFactory.Create(
                Literals.Diagnostic_SingletonCannotContainsNonPrivateCtor,
                TypeSyntax.Identifier);
        }
    }

    private struct GenerationContext(ValidationContext validation)
    {
        private string? _type_FullQualifiedName;
        private string Type_FullQualifiedName => _type_FullQualifiedName ??= validation.TypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        private IdentifierNameSyntax? _type_IdentifierName;
        private IdentifierNameSyntax Type_IdentifierName => _type_IdentifierName ??= SyntaxFactory.IdentifierName(Type_FullQualifiedName);

        private readonly string SingletonProvider_Identifier => validation.SingletonProviderIdentifier ?? Literals.SingletonProvider_TypeIdentifier;


        private ObjectCreationExpressionSyntax GenerateCallCtorExpression()
        {
            return SyntaxFactory.ObjectCreationExpression(
                Type_IdentifierName,
                SyntaxFactory.ArgumentList(),
                null);
        }

        private FieldDeclarationSyntax GenerateProviderFieldDeclaration()
        {
            return SyntaxFactory.FieldDeclaration(
                SyntaxFactory.SingletonList(
                    Literals.Syntax_GeneratedCodeAttribute_AttributeList),
                SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
                SyntaxFactory.VariableDeclaration(
                    Type_IdentifierName,
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.Identifier(Literals.Instance_FieldIdentifier),
                            argumentList: null,
                            SyntaxFactory.EqualsValueClause(
                                GenerateCallCtorExpression())))),
                SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        private ClassDeclarationSyntax GenerateSingletonProviderClassDeclaration()
        {
            return SyntaxFactory.ClassDeclaration(
                SyntaxFactory.SingletonList(
                    Literals.Syntax_GeneratedCodeAttribute_AttributeList),
                SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword)),
                SyntaxFactory.Identifier(SingletonProvider_Identifier),
                typeParameterList: null,
                baseList: null,
                constraintClauses: default,
                SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                    GenerateProviderFieldDeclaration()));
        }

        private PropertyDeclarationSyntax GenerateInstancePropertyDeclaration(bool withProvider)
        {
            AccessorListSyntax? accessorList;
            ArrowExpressionClauseSyntax? expressionBody;
            EqualsValueClauseSyntax? initializer;

            if (withProvider) {
                // Instance => _Provider._Instance;
                accessorList = null;
                expressionBody = SyntaxFactory.ArrowExpressionClause(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName($"{Type_FullQualifiedName}.{SingletonProvider_Identifier}"),
                        SyntaxFactory.IdentifierName($"{Literals.Instance_FieldIdentifier}")));
                initializer = null;
            }
            else {
                // Instance { get; } = new();
                accessorList = SyntaxFactory.AccessorList(
                    SyntaxFactory.SingletonList(
                        SyntaxProvider.AccessorDeclarationNonBody(SyntaxKind.GetAccessorDeclaration)));
                expressionBody = null;
                initializer = SyntaxFactory.EqualsValueClause(
                    GenerateCallCtorExpression());
            }

            return SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.SingletonList(
                    Literals.Syntax_GeneratedCodeAttribute_AttributeList),
                SyntaxFactory.TokenList(
                    SyntaxFactory.Token(
                        validation.Options.HasFlag(ValidationContext.SingletonOptions.IsInternalInstance)
                        ? SyntaxKind.InternalKeyword
                        : SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword)),
                Type_IdentifierName,
                explicitInterfaceSpecifier: null,
                SyntaxFactory.Identifier(validation.InstancePropertyIdentifier ?? Literals.Instance_PropertyIdentifier),
                accessorList,
                expressionBody,
                initializer,
                SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        private ConstructorDeclarationSyntax GenerateNonParameterConstructorDeclaration()
        {
            return SyntaxFactory.ConstructorDeclaration(
                SyntaxFactory.SingletonList(
                    Literals.Syntax_GeneratedCodeAttribute_AttributeList),
                SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword)),
                SyntaxFactory.Identifier(validation.TypeSymbol.Name),
                SyntaxFactory.ParameterList(),
                initializer: null,
                SyntaxFactory.Block());
        }

        private IEnumerable<MemberDeclarationSyntax> GeneratePartialTypeMembers()
        {
            if (!validation.HasPrivateCtor)
                yield return GenerateNonParameterConstructorDeclaration();

            bool withProvider = !validation.Options.HasFlag(ValidationContext.SingletonOptions.NoProvider);
            yield return GenerateInstancePropertyDeclaration(withProvider);

            if (withProvider)
                yield return GenerateSingletonProviderClassDeclaration();
        }

        public CompilationUnitSyntax GenerateCompilationUnit()
        {
            return SyntaxFactory.CompilationUnit(
                default, default, default,
                SyntaxFactory.SingletonList(
                    SyntaxProvider.CloneContainingTypeAndNamespaceDeclarations(
                        validation.TypeSyntax,
                        validation.TypeSymbol,
                        SyntaxFactory.List(
                            GeneratePartialTypeMembers()))));
        }
    }
}
