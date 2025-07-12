using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Trarizon.Library.Collections;
using Trarizon.Library.Roslyn;
using Trarizon.Library.Roslyn.CSharp;
using Trarizon.Library.Roslyn.Emitting;
using Trarizon.Library.Roslyn.Extensions;

namespace Trarizon.Library.Generators;
[Generator(LanguageNames.CSharp)]
internal partial class SingletonGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var filter = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeProxy.Type_FullName,
            (node, token) => node is TypeDeclarationSyntax,
            Emitter.Parse);

        context.RegisterSourceOutputAndPrintDebug(filter);
    }

    private sealed class Emitter(
        ClassDeclarationSyntax syntax,
        INamedTypeSymbol symbol,
        string instancePropertyIdentifier,
        string? providerTypeIdentifier,
        bool shouldEmitPrivateCtor,
        bool isInternalInstance,
        bool hasNewKeyword) : ISourceEmitterWithIndentedWriter
    {
        private bool UseSingletonProvider => providerTypeIdentifier is not null;

        public string GeneratedFileName => $"{symbol.ToValidFileNameString()}.g.cs";

        public static DiagnosticResult<Emitter> Parse(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            if (context.TargetNode is not ClassDeclarationSyntax classSyntax)
                return new DiagnosticData(Diag.SingletonIsClassOnly, (context.TargetNode as TypeDeclarationSyntax)?.Identifier);

            if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
                return default;

            if (classSymbol.IsAbstract || classSymbol.IsStatic)
                return new DiagnosticData(Diag.SingletonCannotBeAbstract, classSyntax.Identifier);

            if (context.Attributes is not [var attrData])
                return default;

            var attr = new AttributeProxy(attrData);

            var instancePropertyIdentifier = attr.InstancePropertyIdentifier;
            if (instancePropertyIdentifier is null)
                instancePropertyIdentifier = AttributeProxy.DefaultInstancePropertyIdentifier;
            else if (!CodeValidation.IsValidIdentifier(instancePropertyIdentifier))
                return new DiagnosticData(Diag.InvalidIdentifier_0Identifier, classSyntax.Identifier, instancePropertyIdentifier);

            cancellationToken.ThrowIfCancellationRequested();

            // 检查基类是否有同名成员
            bool isDuplicateWithBaseMemberName = classSymbol
                .BaseTypes(includeSelf: true)
                .SelectMany(t => t.MemberNames)
                .Contains(instancePropertyIdentifier);

            // Singleton options
            var options = attr.Options;
            string? singleProviderIdentifer;
            if (options.HasFlag(SingletonOptionsMirror.NoProvider)) {
                // Just hide nullable warning
                singleProviderIdentifer = null!;
            }
            else {
                // Provider type identifer
                singleProviderIdentifer = attr.SingletonProviderIdentifier;
                if (singleProviderIdentifer is null)
                    singleProviderIdentifer = AttributeProxy.DefaultSingletonPropertyIdentifier;
                else if (!CodeValidation.IsValidIdentifier(singleProviderIdentifer))
                    return new DiagnosticData(Diag.InvalidIdentifier_0Identifier, classSyntax.Identifier, singleProviderIdentifer);

                if (instancePropertyIdentifier == singleProviderIdentifer)
                    return new DiagnosticData(Diag.SingletonMemberNameRepeat, classSyntax.Identifier);
            }

            cancellationToken.ThrowIfCancellationRequested();

            DiagnosticResult<Emitter> res = new();

            // constructor

            bool hasCustomPrivateCtor = false;

            if (!classSymbol.Constructors.TrySingle(ctor => !ctor.IsStatic).IsSingleOrEmpty(out var first)) {
                res.AddDiagnostics(new DiagnosticData(Diag.D_SingletonHasOneOrNoneCtor, classSyntax.Identifier));
                goto EndCtor;
            }

            if (first is null || first.IsImplicitlyDeclared) {
                goto EndCtor;
            }

            hasCustomPrivateCtor = true;

            // ctor is explicit defined
            var ctor = first;
            if (!(first is null or
                {
                    DeclaredAccessibility: Accessibility.NotApplicable or Accessibility.Private,
                    Parameters.Length: 0
                })) {
                res.AddDiagnostics(new DiagnosticData(Diag.SingletonShouldHaveProperCtor, first.DeclaringSyntaxReferences[0]));
            }

        EndCtor:
            res.Result = new Emitter(classSyntax, classSymbol,
                instancePropertyIdentifier,
                singleProviderIdentifer,
                shouldEmitPrivateCtor: !hasCustomPrivateCtor,
                isInternalInstance: options.HasFlag(SingletonOptionsMirror.IsInternalInstance),
                hasNewKeyword: isDuplicateWithBaseMemberName);
            return res;
        }

        public void Emit(IndentedTextWriter writer)
        {
            writer.WriteLine(Literals.Code_AutoGenerated_TopTrivia);
            writer.WriteLine();

            using (writer.EmitContainingTypesAndNamespaces(symbol, syntax)) {
                writer.WriteLine($"sealed partial class {syntax.Identifier}{syntax.TypeParameterList}");
                using (writer.WriteEnterBracketIndentScope('{')) {
                    if (shouldEmitPrivateCtor) {
                        EmitConstructor(writer);
                        writer.WriteLine();
                    }

                    EmitInstancePropertyDeclaration(writer);

                    if (UseSingletonProvider) {
                        writer.WriteLine();
                        EmitProviderTypeDeclaration(writer);
                    }
                }
            }
        }

        private void EmitConstructor(IndentedTextWriter writer)
        {
            writer.WriteLine(Literals.Code_GeneratedCodeAttributeList);
            writer.WriteLine($"private {symbol.Name}() {{ }}");
        }

        private void EmitInstancePropertyDeclaration(IndentedTextWriter writer)
        {
            writer.WriteLine(Literals.Code_GeneratedCodeAttributeList);

            var accessibility = isInternalInstance ? "internal" : "public";
            string prop = $"{accessibility} static {(hasNewKeyword ? "new " : null)}{symbol.ToFullQualifiedDisplayString()} {instancePropertyIdentifier}";

            if (UseSingletonProvider)
                writer.WriteLine($"{prop} => {symbol.ToFullQualifiedDisplayString()}.{providerTypeIdentifier}.{Instance_Field_Identifier};");
            else
                writer.WriteLine($"{prop} {{ get; }} = new {symbol.ToFullQualifiedDisplayString()}();");
        }

        private void EmitProviderTypeDeclaration(IndentedTextWriter writer)
        {
            Debug.Assert(providerTypeIdentifier is not null);

            writer.WriteLine(Literals.Code_GeneratedCodeAttributeList);
            writer.WriteLine($"private static class {providerTypeIdentifier}");
            using (writer.WriteEnterBracketIndentScope('{')) {
                writer.WriteLine($"public static readonly {symbol.ToFullQualifiedDisplayString()} {Instance_Field_Identifier} = new {symbol.ToFullQualifiedDisplayString()}();");
            }
        }
    }
}
