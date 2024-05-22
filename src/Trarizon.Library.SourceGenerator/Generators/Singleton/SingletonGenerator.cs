﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Threading;
using Trarizon.Library.GeneratorToolkit;
using Trarizon.Library.GeneratorToolkit.Extensions;
using Trarizon.Library.GeneratorToolkit.Helpers;
using Trarizon.Library.GeneratorToolkit.Wrappers;

namespace Trarizon.Library.SourceGenerator.Generators;
[Generator(LanguageNames.CSharp)]
internal sealed partial class SingletonGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var filter = context.SyntaxProvider.ForAttributeWithMetadataName(
            L_Attribute_TypeName,
            (node, token) => node is TypeDeclarationSyntax,
            Emitter.Parse);
            //.OfNotNull();

        context.RegisterSourceOutput(filter);
    }

    private sealed class Emitter(ClassDeclarationSyntax syntax, INamedTypeSymbol symbol,
        string instancePropertyIdentifier, string providerTypeIdentifier, Emitter.SingletonOptions options, bool hasCustomPrivateCtor) : ISourceEmitter
    {
        public static ParseResult<Emitter> Parse(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            if (context.TargetNode is not ClassDeclarationSyntax syntax)
                return new DiagnosticData(D_SingletonIsClassOnly, (context.TargetNode as TypeDeclarationSyntax)?.Identifier);

            if (context.TargetSymbol is not INamedTypeSymbol symbol)
                return default;

            if (symbol.IsAbstract || symbol.IsStatic)
                return new DiagnosticData(D_SingletonCannotBeAbstractOrStatic, syntax.Identifier);

            if (context.Attributes is not [var attribute])
                return default;

            var instancePropertyIdentifier = attribute.GetNamedArgument<string?>(L_Attribute_InstancePropertyName_PropertyIdentifier).Value;
            if (instancePropertyIdentifier is null) {
                instancePropertyIdentifier = L_Instance_PropertyIdentifier;
            }
            else if (!ValidationHelper.IsValidIdentifier(instancePropertyIdentifier))
                return new DiagnosticData(Literals.Diagnostic_InvalidIdentifier_0Identifiers, syntax.Identifier, instancePropertyIdentifier);

            var singletonProviderIdentifier = attribute.GetNamedArgument<string?>(L_Attribute_SingletonProviderName_PropertyIdentifier).Value;
            if (singletonProviderIdentifier is null)
                singletonProviderIdentifier = L_Attribute_SingletonProviderName_PropertyIdentifier;
            else if (!ValidationHelper.IsValidIdentifier(singletonProviderIdentifier))
                return new DiagnosticData(Literals.Diagnostic_InvalidIdentifier_0Identifiers, syntax.Identifier, singletonProviderIdentifier);

            var options = attribute.GetNamedArgument<SingletonOptions>(L_Attribute_Options_PropertyIdentifier).Value;
            if (!options.HasFlag(SingletonOptions.NoProvider) && instancePropertyIdentifier == singletonProviderIdentifier)
                return new DiagnosticData(D_SingletonMemberNameRepeat, syntax.Identifier);

            token.ThrowIfCancellationRequested();

            ParseResult<Emitter> res = new();

            // constructor

            bool hasCustomPrivateCtor = false;

            if (!symbol.Constructors.TrySingleOrNone(ctor => !ctor.IsStatic, out var first)) {
                res.AddDiagnostic(new DiagnosticData(D_SingletonHasOneOrNoneCtor, syntax.Identifier));
                goto EndCtor;
            }

            hasCustomPrivateCtor = first.HasValue && !first.Value.IsImplicitlyDeclared;
            if (!hasCustomPrivateCtor)
                goto EndCtor;

            var ctor = first.Value;
            if (ctor.DeclaredAccessibility is not (Accessibility.NotApplicable or Accessibility.Private)) {
                res.AddDiagnostic(new DiagnosticData(D_SingletonCannotContainsNonPrivateCtor, ctor.DeclaringSyntaxReferences.FirstOrDefault()));
            }

            if (ctor.Parameters.Length > 0) {
                res.AddDiagnostic(new DiagnosticData(D_SingletonCtorHasNoParameter, ctor.DeclaringSyntaxReferences.FirstOrDefault()));
            }

        EndCtor:
            res.Result = new Emitter(syntax, symbol, instancePropertyIdentifier, singletonProviderIdentifier, options, hasCustomPrivateCtor);
            return res;
        }

        public string GenerateFileName()
        {
            return $"{symbol.ToValidFileNameString()}.g.{Literals.SingletonGenerator_Id}.cs";
        }

        public string Emit()
        {
            var sw = new StringWriter();
            var writer = new IndentedTextWriter(sw);

            writer.WriteLine(Literals.AutoGenerated_TopTrivia_Code);
            writer.WriteLine();

            using (writer.EmitContainingTypesAndNamespace(symbol, syntax)) {
                using (writer.WriteLineWithBrace($"sealed partial class {symbol.Name}")) {
                    if (!hasCustomPrivateCtor) {
                        EmitConstructor(writer);
                        writer.WriteLine();
                    }

                    EmitInstancePropertyDeclaration(writer);

                    if (!options.HasFlag(SingletonOptions.NoProvider)) {
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

            var accessibility = options.HasFlag(SingletonOptions.IsInternalInstance) ? "internal" : "public";
            writer.Write($"{accessibility} static {symbol.ToFullQualifiedDisplayString()} {instancePropertyIdentifier} ");

            if (options.HasFlag(SingletonOptions.NoProvider))
                writer.WriteLine($"{{ get; }} = new {symbol.ToFullQualifiedDisplayString()}();");
            else
                writer.WriteLine($"=> {symbol.ToFullQualifiedDisplayString()}.{providerTypeIdentifier}.{L_Instance_FieldIdentifier};");
        }

        private void EmitProviderTypeDeclaration(IndentedTextWriter writer)
        {
            writer.WriteLine(Literals.GeneratedCodeAttributeList_Code);
            using (writer.WriteLineWithBrace($"private static class {providerTypeIdentifier}")) {
                writer.WriteLine($"public static readonly {symbol.ToFullQualifiedDisplayString()} {L_Instance_FieldIdentifier} = new {symbol.ToFullQualifiedDisplayString()}();");
            }
        }

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
    }
}
