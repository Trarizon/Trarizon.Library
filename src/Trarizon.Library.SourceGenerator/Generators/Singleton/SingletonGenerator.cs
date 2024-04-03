using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Trarizon.Library.GeneratorToolkit;
using Trarizon.Library.GeneratorToolkit.Extensions;
using Trarizon.Library.GeneratorToolkit.Helpers;

namespace Trarizon.Library.SourceGenerator.Generators;
[Generator(LanguageNames.CSharp)]
internal sealed partial class SingletonGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var filter = context.SyntaxProvider.ForAttributeWithMetadataName(
            L_Attribute_TypeName,
            (node, token) => node is TypeDeclarationSyntax,
            Emitter.Parse)
            .OfNotNull();

        context.RegisterSourceOutput(filter, (context, result) =>
        {
            if (result is DiagnosticData d) {
                context.ReportDiagnostic(d.ToDiagnostic());
                return;
            }

            var (emitter, diags) = ((Emitter, List<DiagnosticData>?))result;

            foreach (var diag in diags ?? []) {
                context.ReportDiagnostic(diag.ToDiagnostic());
            }
            if (emitter is not null) {
                var str = emitter.Emit();
                Console.WriteLine(str);
                context.AddSource(emitter.GenerateFileName(), emitter.Emit());
            }
        });
    }

    private sealed record class Emitter(ClassDeclarationSyntax Syntax, INamedTypeSymbol Symbol,
        string InstancePropertyIdentifier, string ProviderIdentifier, Emitter.SingletonOptions Options, bool HasCustomPrivateCtor)
    {
        public string InstancePropertyIdentifier = InstancePropertyIdentifier ?? L_Instance_PropertyIdentifier;

        public string ProviderIdentifier = ProviderIdentifier ?? L_SingletonProvider_TypeIdentifier;

        public static object? Parse(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            if (context.TargetNode is not ClassDeclarationSyntax syntax)
                return new DiagnosticData(D_SingletonIsClassOnly, (context.TargetNode as TypeDeclarationSyntax)?.Identifier);

            if (context.TargetSymbol is not INamedTypeSymbol symbol)
                return null;

            if (symbol.IsAbstract || symbol.IsStatic)
                return new DiagnosticData(D_SingletonCannotBeAbstractOrStatic, syntax.Identifier);

            if (context.Attributes is not [var attribute])
                return null;

            var instancePropertyIdentifier = attribute.GetNamedArgument<string?>(L_Attribute_InstancePropertyName_PropertyIdentifier).Value;
            if (instancePropertyIdentifier is null) {
                instancePropertyIdentifier = L_Instance_PropertyIdentifier;
            }
            else if (!ValidationHelper.IsValidIdentifier(instancePropertyIdentifier))
                return new DiagnosticData(D_InvalidInstanceIdentifier, syntax.Identifier);

            var singletonProviderIdentifier = attribute.GetNamedArgument<string?>(L_Attribute_SingletonProviderName_PropertyIdentifier).Value;
            if (singletonProviderIdentifier is null)
                singletonProviderIdentifier = L_Attribute_SingletonProviderName_PropertyIdentifier;
            else if (!ValidationHelper.IsValidIdentifier(singletonProviderIdentifier))
                return new DiagnosticData(D_InvalidSingletonProviderIdentifier, syntax.Identifier);

            var options = attribute.GetNamedArgument<SingletonOptions>(L_Attribute_Options_PropertyIdentifier).Value;
            if (!options.HasFlag(SingletonOptions.NoProvider) && instancePropertyIdentifier == singletonProviderIdentifier)
                return new DiagnosticData(D_SingletonMemberNameRepeat, syntax.Identifier);

            // Confirm generate

            List<DiagnosticData>? diags = null;

            // constructor

            bool hasCustomPrivateCtor = false;

            if (!symbol.Constructors.TrySingleOrNone(ctor => !ctor.IsStatic, out var first)) {
                diags.SafelyAdd(new DiagnosticData(D_SingletonHasOneOrNoneCtor, syntax.Identifier));
                goto EndCtor;
            }

            hasCustomPrivateCtor = first.HasValue && !first.Value.IsImplicitlyDeclared;
            if (!hasCustomPrivateCtor)
                goto EndCtor;

            var ctor = first.Value;
            if (ctor.DeclaredAccessibility is not (Accessibility.NotApplicable or Accessibility.Private)) {
                diags.SafelyAdd(new DiagnosticData(D_SingletonCannotContainsNonPrivateCtor, ctor.DeclaringSyntaxReferences.FirstOrDefault()));
            }

            if (ctor.Parameters.Length > 0) {
                diags.SafelyAdd(new DiagnosticData(D_SingletonCtorHasNoParameter, ctor.DeclaringSyntaxReferences.FirstOrDefault()));
            }

        EndCtor:
            return (new Emitter(syntax, symbol, instancePropertyIdentifier, singletonProviderIdentifier, options, hasCustomPrivateCtor), diags);
        }

        public string GenerateFileName()
        {
            return $"{Symbol.ToValidFileNameString()}.g.{Literals.SingletonGenerator_FileSuffix}.cs";
        }

        public string Emit()
        {
            var sw = new StringWriter();
            var writer = new IndentedTextWriter(sw);

            writer.WriteLine(Literals.AutoGenerated_TopTrivia_Code);
            writer.WriteLine();

            using (writer.EmitContainingTypesAndNamespace(Symbol, Syntax)) {
                using (writer.WriteLineWithBrace($"sealed partial class {Symbol.Name}")) {
                    if (!HasCustomPrivateCtor) {
                        EmitConstructor(writer);
                        writer.WriteLine();
                    }

                    EmitInstancePropertyDeclaration(writer);

                    if (!Options.HasFlag(SingletonOptions.NoProvider)) {
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
            writer.WriteLine($"private {Symbol.Name}() {{ }}");
        }

        private void EmitInstancePropertyDeclaration(IndentedTextWriter writer)
        {
            writer.WriteLine(Literals.GeneratedCodeAttributeList_Code);

            var accessibility = Options.HasFlag(SingletonOptions.IsInternalInstance) ? "internal" : "public";
            writer.Write($"{accessibility} static {Symbol.ToFullQualifiedDisplayString()} {InstancePropertyIdentifier} ");

            if (Options.HasFlag(SingletonOptions.NoProvider))
                writer.WriteLine($"{{ get; }} = new {Symbol.ToFullQualifiedDisplayString()}();");
            else
                writer.WriteLine($"=> {Symbol.ToFullQualifiedDisplayString()}.{ProviderIdentifier}.{L_Instance_FieldIdentifier};");
        }

        private void EmitProviderTypeDeclaration(IndentedTextWriter writer)
        {
            writer.WriteLine(Literals.GeneratedCodeAttributeList_Code);
            using (writer.WriteLineWithBrace($"private static class {ProviderIdentifier}")) {
                writer.WriteLine($"public static readonly {Symbol.ToFullQualifiedDisplayString()} {L_Instance_FieldIdentifier} = new {Symbol.ToFullQualifiedDisplayString()}();");
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
