using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Trarizon.Library.GeneratorToolkit;
using Trarizon.Library.GeneratorToolkit.Extensions;
using Trarizon.Library.GeneratorToolkit.Helpers;
using Trarizon.Library.GeneratorToolkit.More;

namespace Trarizon.Library.SourceGenerator.Generators;
// [Generator(LanguageNames.CSharp)]
internal sealed partial class TaggedUnionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var filter = context.SyntaxProvider.ForAttributeWithMetadataName(
            L_Attribute_TypeName,
            (node, token) => node is EnumDeclarationSyntax,
            Emitter.Parse)
            .OfNotNull();

        context.RegisterSourceOutput(filter, (context, result) =>
        {
            if (result is DiagnosticData d) {
                context.ReportDiagnostic(d.ToDiagnostic());
                return;
            }

            if (result is List<DiagnosticData> ds) {
                foreach (var diag in ds) {
                    context.ReportDiagnostic(diag.ToDiagnostic());
                }
                return;
            }

            if (result is Emitter e) {
                Console.WriteLine(e.Emit());
                context.AddSource(e.GenerateFileName(), e.Emit());
                return;
            }

            var (emitter, diags) = ((Emitter, List<DiagnosticData>?))result;

            foreach (var diag in diags ?? []) {
                context.ReportDiagnostic(diag.ToDiagnostic());
            }
            if (emitter is not null) {
                Console.WriteLine(emitter.Emit());
                context.AddSource(emitter.GenerateFileName(), emitter.Emit());
            }
        });
    }

    private sealed class Emitter(EnumDeclarationSyntax syntax, INamedTypeSymbol symbol,
        string taggedUnionTypeIdentifier, IEnumerable<Variant> variants, int maxReferenceCount)
    {
        public string GenerateFileName() => $"{symbol.ToValidFileNameString()}.g.{Literals.TaggedUnionGenerator_Id}.cs";

        public static object? Parse(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            if (context.TargetNode is not EnumDeclarationSyntax syntax)
                return null;

            if (context.TargetSymbol is not INamedTypeSymbol { TypeKind: TypeKind.Enum } symbol)
                return null;

            if (context.Attributes is not [var attribute])
                return null;

            var generatedTypeIdentifier = attribute.GetConstructorArgument<string?>(L_Attribute_GeneratedTypeName_ConstructorIndex).Value;
            if (generatedTypeIdentifier is null) {
                generatedTypeIdentifier = GetDefaultGeneratedTypeIdentifier(symbol);
            }
            else if (!ValidationHelper.IsValidIdentifier(generatedTypeIdentifier))
                return new DiagnosticData(Literals.Diagnostic_InvalidIdentifier_0Identifiers, syntax.Identifier.GetLocation(), generatedTypeIdentifier);

            var variants = symbol.GetMembers()
                .OfType<IFieldSymbol>()
                // Collect TagVariant attribute datas
                .Select(field =>
                {
                    var attr = field.GetAttributes()
                       .Where(attr => attr.AttributeClass?.ToDisplayString(MoreSymbolDisplayFormats.DefaultWithoutGeneric) is L_VariantAttribute_TypeName)
                       .FirstOrDefault();
                    if (attr is null)
                        return new Variant(field, ImmutableArray<ITypeSymbol>.Empty, ImmutableArray<string>.Empty);

                    ImmutableArray<ITypeSymbol> types;
                    IEnumerable<string?> identifiers;

                    if (attr.AttributeClass!.TypeParameters.IsEmpty) {
                        types = attr.GetConstructorArguments<ITypeSymbol>(L_VariantAttribute_Types_ConstructorIndex);
                        identifiers = attr.GetNamedArguments<string?>(L_VariantAttribute_Identifiers_PropertyIdentifier);
                    }
                    else {
                        types = attr.AttributeClass!.TypeArguments;
                        identifiers = attr.ConstructorArguments
                            .Select(arg => arg.Value)
                            .Cast<string?>()
                            .ToImmutableArray();
                    }
                    return new Variant(field,
                        types.EmptyIfDefault(),
                        identifiers.Select((arg, i) => arg ?? $"Item{i + 1}").ToImmutableArray());
                })
                .ToList();

            // Distinct enum values
            var diags = variants
                .DuplicatesBy(v => v.EnumValue)
                .Select(v => new DiagnosticData(D_EnumValueRepeat, v.EnumField.DeclaringSyntaxReferences[0]))
                .ToListIfAny();
            if (diags is not null)
                return diags;

            // Distinct identifiers
            diags = variants
                .Where(v => !v.Fields.Identifiers.IsDistinct())
                .Select(v => new DiagnosticData(D_VariantFieldNameRepeat, v.EnumField.DeclaringSyntaxReferences[0]))
                .ToListIfAny();
            if (diags is not null)
                return diags;

            int max = variants.Max(v => v.ReferenceCount);

            if (!variants.Any(v => v is { EnumValue: 0L, Fields.Length: 0 }))
                (diags ??= []).Add(new DiagnosticData(D_ProvideNonVariantZeroField, syntax.Identifier.GetLocation()));
            return (new Emitter(syntax, symbol, generatedTypeIdentifier, variants, max), diags);
        }

        private static string GetDefaultGeneratedTypeIdentifier(INamedTypeSymbol symbol)
        {
            var enumName = symbol.Name;
            if (enumName.EndsWith("Kind"))
                return enumName[..^4];
            else
                return $"{enumName}Union";
        }

        public string Emit()
        {
            var sw = new StringWriter();
            var writer = new IndentedTextWriter(sw);

            writer.WriteLine(Literals.AutoGenerated_TopTrivia_Code);
            writer.WriteLine();

            using (writer.EmitContainingTypesAndNamespace(symbol, syntax)) {
                using (writer.WriteLineWithBrace($"{syntax.Modifiers} partial struct {taggedUnionTypeIdentifier}")) {
                    writer.WriteLine(Literals.GeneratedCodeAttributeList_Code);
                    writer.WriteLine($"public readonly {symbol.ToFullQualifiedDisplayString()} {L_Tag_PropertyIdentifier} {{ get; private init; }}");

                    writer.WriteLine(Literals.GeneratedCodeAttributeList_Code);
                    writer.WriteLine($"private {L_Struct_TypeIdentifier} {L_Struct_FieldIdentifier};");

                    foreach (var variant in variants) {
                        writer.WriteLine();
                        EmitCreateMethod(writer, variant);
                        writer.WriteLine();
                        EmitTryGetMethod(writer, variant);
                    }

                    writer.WriteLine();
                    EmitUnionStruct(writer);
                }
            }


            return sw.ToString();
        }

        private void EmitUnionStruct(IndentedTextWriter writer)
        {
            writer.WriteLine(Literals.GeneratedCodeAttributeList_Code);
            writer.WriteLine($"[global::{Globals.StructLayoutAttribute_TypeName}(global::{Globals.LayoutKind_Explicit_EnumValue})]");
            using (writer.WriteLineWithBrace($"private struct {L_Struct_TypeIdentifier}")) {
                // We do not generate type for simple enum field with no variant
                foreach (var variant in variants.Where(v => !v.Fields.IsEmpty)) {
                    writer.WriteLine($"[global::{Globals.FieldOffsetAttribute_TypeName}(0)]");
                    writer.WriteLine($"public {GenerateVariantTypeName(variant)} {variant.EnumField.Name};");
                }
            }
        }

        private void EmitCreateMethod(IndentedTextWriter writer, Variant variant)
        {
            var parameters = variant.Fields
                .Select(v => $"{v.Type.ToFullQualifiedDisplayString()} {v.Identifier}");
            using (writer.WriteLineWithIndent($"public static {taggedUnionTypeIdentifier} Create{variant.EnumField.Name}({string.Join(", ", parameters)}) => new()", "{", "};")) {
                writer.WriteLine($"{L_Tag_PropertyIdentifier} = {symbol.ToFullQualifiedDisplayString()}.{variant.EnumField.Name},");
                if (variant.Fields.IsEmpty)
                    return;

                using (writer.WriteLineWithIndent($"{L_Struct_FieldIdentifier} = new()", "{", "},")) {
                    writer.WriteLine($"{variant.EnumField.Name} = {GenerateVariantCreate(variant)},");
                }
            }
        }

        private void EmitTryGetMethod(IndentedTextWriter writer, Variant variant)
        {
            if (variant.Fields.IsEmpty)
                return;

            IEnumerable<string> parameters = variant.Fields
                .Select((v) =>
                {
                    var attr = v.Type.IsReferenceType ? $"[{Globals.MaybeNullWhenAttribute_TypeName}(false)] " : "";
                    return $"{attr}out {v.Type.ToFullQualifiedDisplayString()} {v.Identifier}";
                });

            using (writer.WriteLineWithBrace($"public readonly bool TryGet{variant.EnumField.Name}({string.Join(", ", parameters)})")) {
                using (writer.WriteLineWithBrace($"if ({L_Tag_PropertyIdentifier} is {symbol.ToFullQualifiedDisplayString()}.{variant.EnumField.Name})")) {
                    foreach (var (index, iden) in variant.Fields.Identifiers.Index()) {
                        writer.WriteLine($"{iden} = {L_Struct_FieldIdentifier}.{GenerateVariantAccess(variant, index)};");
                    }
                    writer.WriteLine("return true;");
                }
                foreach (var iden in variant.Fields.Identifiers) {
                    writer.WriteLine($"{iden} = default;");
                }
                writer.WriteLine("return false;");
            }
        }

        // For reference type, we use type itself
        // For value type, we use ValueTuple to padding necessary count of object to match the requirement
        private string GenerateVariantTypeName(Variant variant)
        {
            Debug.Assert(!variant.Fields.IsEmpty);

            string variantType;
            if (variant.Fields.Length == 1)
                variantType = variant.Fields[0].Type.ToFullQualifiedDisplayString();
            else
                variantType = $"({string.Join(", ", variant.Fields.Types.Select(type => type.ToFullQualifiedDisplayString()))})";

            if (variant.IsSingleReferenceType || variant.ReferenceCount == maxReferenceCount)
                return variantType;
            return $"({variantType}{string.Concat(Enumerable.Repeat(", object", maxReferenceCount - variant.ReferenceCount))})";
        }

        private string GenerateVariantCreate(Variant variant)
        {
            // Keep sync with GenerateVariantTypeName
            if (variant.IsSingleReferenceType)
                return variant.Fields.Identifiers[0];

            string variantFields;
            if (variant.Fields.Length == 1)
                variantFields = variant.Fields.Identifiers[0];
            else
                variantFields = $"({string.Join(", ", variant.Fields.Identifiers)})";

            if (variant.ReferenceCount == maxReferenceCount)
                return variantFields;
            else
                return $"new() {{ Item1 = {variantFields}, }}";
        }

        private string GenerateVariantAccess(Variant variant, int index = 0)
        {
            if (variant.IsSingleReferenceType)
                return variant.EnumField.Name;

            string variantAccessField;
            if (variant.Fields.Length == 1)
                variantAccessField = "";
            else
                variantAccessField = $".Item{index + 1}";

            if (variant.ReferenceCount == maxReferenceCount)
                return $"{variant.EnumField.Name}{variantAccessField}";
            else
                return $"{variant.EnumField.Name}.Item1{variantAccessField}";
        }
    }

    private sealed class Variant
    {
        public IFieldSymbol EnumField { get; }

        public long EnumValue { get; }

        public FieldZipCollection Fields { get; }

        public int ReferenceCount { get; }

        public bool IsSingleReferenceType => ReferenceCount == -1;

        public Variant(IFieldSymbol enumField, ImmutableArray<ITypeSymbol> types, ImmutableArray<string> identifiers)
        {
            EnumField = enumField;
            EnumValue = enumField.ConstantValue switch {
                byte b => b,
                sbyte sb => sb,
                short s => s,
                ushort us => us,
                int i => i,
                uint ui => ui,
                long l => l,
                ulong ul => Unsafe.As<ulong, long>(ref ul),
                _ => throw new InvalidCastException()
            };
            Fields = new(types, identifiers);

            if (Fields.IsEmpty)
                ReferenceCount = -2;
            else if (Fields is not [(var singleType, _)])
                ReferenceCount = types.Aggregate(0, (count, type) => count + ReferenceFieldCount(type));
            else if (singleType.IsReferenceType)
                ReferenceCount = -1;
            else if (singleType.IsUnmanagedType)
                ReferenceCount = 0;
            else
                ReferenceCount = ReferenceFieldCount(singleType);
        }

        private static int ReferenceFieldCount(ITypeSymbol symbol)
        {
            if (symbol.IsReferenceType)
                return 1;
            if (symbol.IsUnmanagedType)
                return 0;
            return symbol.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(f => !f.IsExplicitlyNamedTupleElement)
                .Aggregate(0, (count, field) => count + ReferenceFieldCount(field.Type));
        }

        public readonly struct FieldZipCollection(ImmutableArray<ITypeSymbol> types, ImmutableArray<string> identifiers) : IEnumerable<(ITypeSymbol Type, string Identifier)>
        {
            public ImmutableArray<ITypeSymbol> Types { get; } = types;
            public ImmutableArray<string> Identifiers { get; } = identifiers;

            public int Length => Types.Length;
            public bool IsEmpty => Types.IsEmpty;
            public (ITypeSymbol Type, string Identifier) this[int index]
            {
                get {
                    var type = Types[index];
                    var identifier = Identifiers.TryAt(index).Value ?? $"Item{index + 1}";
                    return (type, identifier);
                }
            }

            public IEnumerator<(ITypeSymbol Type, string Identifier)> GetEnumerator()
            {
                for (int i = 0; i < Length; i++) {
                    yield return this[i];
                }
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
