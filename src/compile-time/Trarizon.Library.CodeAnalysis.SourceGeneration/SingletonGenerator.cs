using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Trarizon.Library.CodeAnalysis.SourceGeneration.Literals;
using Trarizon.Library.Collections;
using Trarizon.Library.Roslyn.CSharp;
using Trarizon.Library.Roslyn.Diagnostics;
using Trarizon.Library.Roslyn.Emitting;
using Trarizon.Library.Roslyn.Extensions;

namespace Trarizon.Library.CodeAnalysis.SourceGeneration;
[Generator(LanguageNames.CSharp)]
internal partial class SingletonGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var filter = context.SyntaxProvider.ForAttributeWithMetadataName(
            RuntimeAttribute.TypeFullName,
            (n, _) => n is TypeDeclarationSyntax,
            Emitter.Parse);

        context.RegisterSourceOutput(filter);
    }

    private sealed class Emitter(
        ClassDeclarationSyntax syntax,
        INamedTypeSymbol symbol,
        string instancePropertyIdentifier,
        string? providerTypeIdentifier,
        bool shouldEmitPrivateCtor,
        SingletonAccessibility instanceAccessibility,
        bool hasNewKeyword) : ISourceEmitter
    {
        public static DiagnosticResult<Emitter> Parse(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
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

            cancellationToken.ThrowIfCancellationRequested();

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

            cancellationToken.ThrowIfCancellationRequested();

            var res = new DiagnosticResult<Emitter>();

            // constructor

            bool hasCustomPrivateCtor = false;

            if (!symbol.Constructors.TrySingle(ctor => !ctor.IsStatic).IsSingleOrEmpty(out var first)) {
                res.AppendDiagnostic(new DiagnosticData(Descriptors.SingletonShouldHaveCorrectCtor, syntax.Identifier));
                goto EndCtor;
            }
            if (first is null || first.IsImplicitlyDeclared) {
                goto EndCtor;
            }
            hasCustomPrivateCtor = true;

            // ctor is explicit defined
            var ctor = first;
            if (first is not
                {
                    DeclaredAccessibility: Accessibility.NotApplicable or Accessibility.Private,
                    Parameters.Length: 0
                }) {
                res.AppendDiagnostic(new DiagnosticData(Descriptors.SingletonShouldHaveCorrectCtor, syntax.Identifier));
                goto EndCtor;
            }

        EndCtor:
            return res.SelectValue(new Emitter(syntax, symbol,
                instancePropertyIdentifier: instancePropertyIdentifier,
                providerTypeIdentifier: singletonProviderIdentifier,
                shouldEmitPrivateCtor: !hasCustomPrivateCtor,
                instanceAccessibility: instanceAccessibility,
                hasNewKeyword: isBaseTypeHasMember));
        }

        private bool UseSingletonProvider => providerTypeIdentifier is not null;

        private const string ProviderInstanceField = "Instance";

        public string GeneratedFileName => $"{symbol.ToValidFileNameString()}.g.cs";

        public void Emit(IndentedTextWriter writer)
        {
            writer.WriteLine(Codes.AutoGeneratedTopTrivia);
            writer.WriteLine();

            using (writer.EmitContainingTypesAndNamespaces(symbol, syntax)) {
                writer.WriteLine($"sealed partial class {syntax.Identifier}{syntax.TypeParameterList}");
                using (writer.WriteEnterBracketIndentScope('{')) {
                    if (shouldEmitPrivateCtor) {
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
            writer.WriteLine($"private {symbol.Name}() {{ }}");
        }

        private void EmitInstanceProperty(IndentedTextWriter writer)
        {
            writer.WriteLine(Codes.GeneratedCodeAttributeList);
            var accessibility = instanceAccessibility switch
            {
                SingletonAccessibility.Public => "public",
                SingletonAccessibility.Internal => "internal",
                SingletonAccessibility.Protected => "protected",
                SingletonAccessibility.Private => "private",
                SingletonAccessibility.PrivateProtected => "private protected",
                SingletonAccessibility.ProtectedInternal => "protected internal",
                _ => "public",
            };
            var @new = hasNewKeyword ? "new " : null;
            string prop = $"{accessibility} static {@new}{symbol.ToFullQualifiedDisplayString()} {instancePropertyIdentifier}";

            if (UseSingletonProvider)
                writer.WriteLine($"{prop} => {symbol.ToFullQualifiedDisplayString()}.{providerTypeIdentifier}.{ProviderInstanceField};");
            else
                writer.WriteLine($"{prop} {{ get; }} = new {symbol.ToFullQualifiedDisplayString()}();");
        }

        private void EmitSingletonProvider(IndentedTextWriter writer)
        {
            Debug.Assert(providerTypeIdentifier is not null);
            writer.WriteLine(Codes.GeneratedCodeAttributeList);
            writer.WriteLine($"private static class {providerTypeIdentifier}");
            using (writer.WriteEnterBracketIndentScope('{')) {
                writer.WriteLine($"public static readonly {symbol.ToFullQualifiedDisplayString()} {ProviderInstanceField} = new {symbol.ToFullQualifiedDisplayString()}();");
            }
        }
    }

}
