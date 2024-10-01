using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Threading;
using Trarizon.Library.GeneratorToolkit;
using Trarizon.Library.GeneratorToolkit.ContextModelExtensions;
using Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
using Trarizon.Library.GeneratorToolkit.CSharp;
using Trarizon.Library.GeneratorToolkit.Wrappers;
using static Trarizon.Library.SourceGeneration.Generators.SingletonLiterals;

namespace Trarizon.Library.SourceGeneration.Generators;
[Generator(LanguageNames.CSharp)]
internal sealed class SingletonGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var filter = context.SyntaxProvider.ForAttributeWithMetadataName(
            L_Attribute_TypeName,
            (node, token) => node is TypeDeclarationSyntax,
            Emitter.Parse);

        context.RegisterSourceOutputAndPrintDebug(filter);
    }

    private sealed class Emitter(ClassDeclarationSyntax syntax, INamedTypeSymbol symbol,
        string instancePropertyIdentifier, string providerTypeIdentifier, SingletonOptions_Mirror options,
        bool hasCustomPrivateCtor, bool hasNewKeyword) : ISourceEmitter
    {
        public static ParseResult<ISourceEmitter> Parse(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            if (context.TargetNode is not ClassDeclarationSyntax classSyntax)
                return new DiagnosticData(D_SingletonIsClassOnly, (context.TargetNode as TypeDeclarationSyntax)?.Identifier);

            if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
                return default;

            if (classSymbol.IsAbstract || classSymbol.IsStatic)
                return new DiagnosticData(D_SingletonCannotBeAbstractOrStatic, classSyntax.Identifier);

            if (context.Attributes is not [var attribute])
                return default;
            
            // Instance property identifier
            var instancePropertyIdentifier = attribute.GetNamedArgument<string?>(L_Attribute_InstancePropertyName_PropertyIdentifier).Value;
            if (instancePropertyIdentifier is null) {
                instancePropertyIdentifier = L_Instance_PropertyIdentifier;
            }
            else if (!CodeValidation.IsValidIdentifier(instancePropertyIdentifier))
                return new DiagnosticData(Literals.Diagnostic_InvalidIdentifier_0Identifiers, classSyntax.Identifier, instancePropertyIdentifier);

            token.ThrowIfCancellationRequested();

            // 检查基类是否有同名成员
            bool isDuplicateWithBaseMemberName = classSymbol
                .EnumerateByWhileNotNull(s => s.BaseType)
                .SelectMany(t => t.MemberNames)
                .Any(name => name == instancePropertyIdentifier);

            // Singleton options
            var options = attribute.GetNamedArgument<SingletonOptions_Mirror>(L_Attribute_Options_PropertyIdentifier).Value;
            string? singletonProviderIdentifier;
            if (options.HasFlag(SingletonOptions_Mirror.NoProvider)) {
                // Just hide nullable warning, actually this value won't be used if NoProvider
                singletonProviderIdentifier = L_SingletonProvider_TypeIdentifier;
            }
            else {
                // Provider type identifier
                singletonProviderIdentifier = attribute.GetNamedArgument<string?>(L_Attribute_SingletonProviderName_PropertyIdentifier).Value;
                if (singletonProviderIdentifier is null)
                    singletonProviderIdentifier = L_SingletonProvider_TypeIdentifier;
                else if (!CodeValidation.IsValidIdentifier(singletonProviderIdentifier))
                    return new DiagnosticData(Literals.Diagnostic_InvalidIdentifier_0Identifiers, classSyntax.Identifier, singletonProviderIdentifier);

                if (instancePropertyIdentifier == singletonProviderIdentifier)
                    return new DiagnosticData(D_SingletonMemberNameRepeat, classSyntax.Identifier);
            }

            token.ThrowIfCancellationRequested();

            ParseResult<ISourceEmitter> res = new();

            // constructor

            bool hasCustomPrivateCtor = false;

            if (!classSymbol.Constructors.TrySingle(ctor => !ctor.IsStatic).IsSingleOrEmpty(out var first)) {
                res.AddDiagnostic(new DiagnosticData(D_SingletonHasOneOrNoneCtor, classSyntax.Identifier));
                goto EndCtor;
            }

            if (first is null || first.IsImplicitlyDeclared) {
                goto EndCtor;
            }

            hasCustomPrivateCtor = true;

            // ctor is explicit defined 
            var ctor = first;
            if (ctor.DeclaredAccessibility is not (Accessibility.NotApplicable or Accessibility.Private)) {
                res.AddDiagnostic(new DiagnosticData(D_SingletonCannotContainsNonPrivateCtor, ctor.DeclaringSyntaxReferences.FirstOrDefault()));
            }

            if (ctor.Parameters.Length > 0) {
                res.AddDiagnostic(new DiagnosticData(D_SingletonCtorHasNoParameter, ctor.DeclaringSyntaxReferences.FirstOrDefault()));
            }

        EndCtor:
            res.Result = new Emitter(classSyntax, classSymbol, instancePropertyIdentifier, singletonProviderIdentifier, options, hasCustomPrivateCtor, isDuplicateWithBaseMemberName);
            return res;
        }

        public string GenerateFileName()
        {
            return $"{symbol.ToValidFileNameString()}.g.cs";
        }

        public string Emit()
        {
            var sw = new StringWriter();
            var writer = new IndentedTextWriter(sw);

            writer.WriteLine(Literals.AutoGenerated_TopTrivia_Code);
            writer.WriteLine();

            using (writer.EmitContainingTypesAndNamespaces(symbol, syntax)) {
                writer.WriteLine($"sealed partial class {syntax.Identifier}{syntax.TypeParameterList}");
                using (writer.WriteBracketIndentScope('{')) {
                    if (!hasCustomPrivateCtor) {
                        EmitConstructor(writer);
                        writer.WriteLine();
                    }

                    EmitInstancePropertyDeclaration(writer);

                    if (!options.HasFlag(SingletonOptions_Mirror.NoProvider)) {
                        writer.WriteLine();
                        EmitProviderTypeDeclaration(writer);
                    }
                }
            }

            return sw.ToString();
        }

        private void EmitConstructor(IndentedTextWriter writer)
        {
            writer.WriteLine(Literals.GeneratedCodeAttributeList_Code);
            writer.WriteLine($"private {symbol.Name}() {{ }}");
        }

        private void EmitInstancePropertyDeclaration(IndentedTextWriter writer)
        {
            writer.WriteLine(Literals.GeneratedCodeAttributeList_Code);

            var accessibility = options.HasFlag(SingletonOptions_Mirror.IsInternalInstance) ? "internal" : "public";
            writer.Write($"{accessibility} static {(hasNewKeyword ? "new " : null)}{symbol.ToFullQualifiedDisplayString()} {instancePropertyIdentifier} ");

            if (options.HasFlag(SingletonOptions_Mirror.NoProvider))
                writer.WriteLine($"{{ get; }} = new {symbol.ToFullQualifiedDisplayString()}();");
            else
                writer.WriteLine($"=> {symbol.ToFullQualifiedDisplayString()}.{providerTypeIdentifier}.{L_Instance_FieldIdentifier};");
        }

        private void EmitProviderTypeDeclaration(IndentedTextWriter writer)
        {
            writer.WriteLine(Literals.GeneratedCodeAttributeList_Code);
            writer.WriteLine($"private static class {providerTypeIdentifier}");
            using (writer.WriteBracketIndentScope('{')) {
                writer.WriteLine($"public static readonly {symbol.ToFullQualifiedDisplayString()} {L_Instance_FieldIdentifier} = new {symbol.ToFullQualifiedDisplayString()}();");
            }
        }
    }
}
