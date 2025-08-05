using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Trarizon.Library.CodeAnalysis.SourceGeneration.Literals;
using Trarizon.Library.Collections;
using Trarizon.Library.Functional;
using Trarizon.Library.Roslyn.Diagnostics;
using Trarizon.Library.Roslyn.Extensions;
using Trarizon.Library.Roslyn.SourceInfos;
using Trarizon.Library.Roslyn.SourceInfos.CSharp;
using Trarizon.Library.Roslyn.SourceInfos.CSharp.Emitting;
using Trarizon.Library.Roslyn.SourceInfos.Emitting;

namespace Trarizon.Library.CodeAnalysis.SourceGeneration;
[Generator(LanguageNames.CSharp)]
internal partial class SingletonGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var filter = context.SyntaxProvider.ForAttributeWithMetadataName(
            RuntimeAttribute.TypeFullName,
            (n, _) => n is TypeDeclarationSyntax,
            Parse)
            .OfNotNull();

        context.RegisterSourceOutput(filter, (ctx, src) =>
        {
            if (!src.TryGetValue(out var emitter, out var diag)) {
                ctx.ReportDiagnostic(diag.ToDiagnostic());
            }
            else {
                ctx.AddSource(emitter.GeneratedFileName, emitter.Emit());
            }
        });
    }

    private Result<EmitModel, DiagnosticData>? Parse(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.TargetNode is not ClassDeclarationSyntax syntax)
            return new DiagnosticData(Descriptors.OnlyClassCanBeSingleton, ((TypeDeclarationSyntax)context.TargetNode).Identifier);

        if (context.TargetSymbol is not INamedTypeSymbol symbol)
            return default;

        if (symbol.IsAbstract || symbol.IsStatic)
            return new DiagnosticData(Descriptors.SingletonShouldBeSealed, syntax.Identifier);

        if (context.Attributes is not [var attr])
            return default;

        var attribute = new RuntimeAttribute(attr);
        var instancePropertyIdentifier = attribute.GetInstancePropertyName();
        if (instancePropertyIdentifier is null)
            instancePropertyIdentifier = RuntimeAttribute.DefaultInstancePropertyName;
        else if (!CodeValidation.IsValidIdentifier(instancePropertyIdentifier))
            return new DiagnosticData(Descriptors.InvalidIdentifier, syntax.Identifier, instancePropertyIdentifier);

        // Check if base type has member with same name;

        bool isBaseTypeHasMember = symbol.BaseTypes()
            .SelectMany(t => t.MemberNames)
            .Contains(instancePropertyIdentifier);

        // Singleton options
        string? singletonProviderIdentifier;
        if (attribute.GetGenerateProvider()) {
            singletonProviderIdentifier = attribute.GetSingletonProviderName();
            if (singletonProviderIdentifier is null)
                singletonProviderIdentifier = RuntimeAttribute.DefaultSingletonProviderName;
            else if (!CodeValidation.IsValidIdentifier(singletonProviderIdentifier))
                return new DiagnosticData(Descriptors.InvalidIdentifier, syntax.Identifier, singletonProviderIdentifier);

            if (instancePropertyIdentifier == singletonProviderIdentifier)
                return new DiagnosticData(Descriptors.InstancePropertyNameAndSingletonProviderNameShouldBeDifferent, syntax.Identifier);
        }
        else {
            singletonProviderIdentifier = null;
        }

        var instanceAccessibility = attribute.GetInstanceAccessibility();

        //var res = new DiagnosticResult<EmitModel>();
        var diags = new List<DiagnosticData>();

        // constructor

        bool hasCustomPrivateCtor = false;

        // Declared multiple constructors
        if (!symbol.Constructors.TrySingle(ctor => !ctor.IsStatic).IsSingleOrEmpty(out var first)) {
            return new DiagnosticData(Descriptors.SingletonShouldHaveCorrectCtor, syntax.Identifier);
        }

        // No declared ctor, ok
        if (first is null || first.IsImplicitlyDeclared) {
            goto EndCtor;
        }

        // ctor is explicit defined
        hasCustomPrivateCtor = true;
        var ctor = first;
        if (first is not
            {
                DeclaredAccessibility: Accessibility.NotApplicable or Accessibility.Private,
                Parameters.Length: 0
            }) {
            // Not private non-param ctor
            return new DiagnosticData(Descriptors.SingletonShouldHaveCorrectCtor, syntax.Identifier);
        }

    EndCtor:

        return new EmitModel(
            CodeInfoFactory.GetTypeDeclaration(symbol, syntax),
            symbol.Name,
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            $"{symbol.ToValidFileNameString()}.g.cs",
            instancePropertyIdentifier,
            singletonProviderIdentifier,
            EmitPrivateCtor: !hasCustomPrivateCtor,
            InstancePropertyAccessbility: instanceAccessibility,
            RequiresNewKeyword: isBaseTypeHasMember);
    }

    private sealed record EmitModel(
        TypeDeclarationCodeInfo TypeDecl,
        string TypeName,
        string FullQualifiedTypeName,
        string GeneratedFileName,
        string InstancePropertyIdentifier,
        string? ProviderTypeIdentifier,
        bool EmitPrivateCtor,
        SingletonAccessibility InstancePropertyAccessbility,
        bool RequiresNewKeyword)
        : ISourceEmitter
    {
        private const string ProviderInstanceFieldIdentifier = "Instance";

        public bool UseSingletonProvider => ProviderTypeIdentifier is not null;

        public void Emit(IndentedTextWriter writer)
        {
            writer.WriteLine(CodeFactory.AutoGeneratedTopCommentTrivia);
            writer.WriteLine();

            using (writer.EmitCSharpPartialTypeAndContainingTypeAndNamespaces(TypeDecl.Parent)) {
                writer.WriteLine($"sealed partial class {TypeDecl.Name}");
                using (writer.EnterBracketDedentScope('{')) {
                    if (EmitPrivateCtor) {
                        EmitConstructor(writer);
                        writer.WriteLine();
                    }

                    EmitInstanceProperty(writer);

                    if (UseSingletonProvider) {
                        writer.WriteLine();
                        EmitSingletonProvider(writer);
                    }
                }
            }
        }

        private void EmitConstructor(IndentedTextWriter writer)
        {
            writer.WriteLine(Codes.GeneratedCodeAttributeList);
            writer.WriteLine($"private {TypeName}() {{ }}");
        }

        private void EmitInstanceProperty(IndentedTextWriter writer)
        {
            writer.WriteLine(Codes.GeneratedCodeAttributeList);
            var accessibility = InstancePropertyAccessbility switch
            {
                SingletonAccessibility.Public => "public",
                SingletonAccessibility.Internal => "internal",
                SingletonAccessibility.Protected => "protected",
                SingletonAccessibility.Private => "private",
                SingletonAccessibility.PrivateProtected => "private protected",
                SingletonAccessibility.ProtectedInternal => "protected internal",
                _ => "public",
            };
            var @new = RequiresNewKeyword ? "new " : null;
            string prop = $"{accessibility} static {@new}{FullQualifiedTypeName} {InstancePropertyIdentifier}";

            if (UseSingletonProvider)
                writer.WriteLine($"{prop} => {FullQualifiedTypeName}.{ProviderTypeIdentifier}.{ProviderInstanceFieldIdentifier};");
            else
                writer.WriteLine($"{prop} {{ get; }} = new {FullQualifiedTypeName}();");
        }

        private void EmitSingletonProvider(IndentedTextWriter writer)
        {
            Debug.Assert(ProviderTypeIdentifier is not null);
            writer.WriteLine(Codes.GeneratedCodeAttributeList);
            writer.WriteLine($"private static class {ProviderTypeIdentifier}");
            using (writer.EnterBracketDedentScope('{')) {
                writer.WriteLine($"public static readonly {FullQualifiedTypeName} {ProviderInstanceFieldIdentifier} = new {FullQualifiedTypeName}();");
            }
        }
    }
}
