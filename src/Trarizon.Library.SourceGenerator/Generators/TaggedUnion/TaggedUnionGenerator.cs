/**Options
 * - UnionTypeKind: 默认Struct，有任何一个字段为ref struct时为RefStruct
 *   - Struct: 默认的TaggedUnion
 *   - Class: 使用class和type pattern match check
 *   - Record: 使用Record与type pattern match check
 *   - RefStruct：一个tag和一个ref byte  // 安全性存疑，暂定
 * - Pack: 用于指定[StructLayout]的Pack
 * - CreatorAccessibility: Create方法或ctor的accessibility
 *   - 对Record无效
 * - // AggressiveMinimizingSize:试图不存储tag并通过字段获取tag
 *   - 因为获取tag时性能问题似乎不小所以取消了
 *   - 但是TryGetXXX里使用或许可以，暂定
 */

/**partial struct Union
 * {
 *     object __obj0;
 *     object __obj1;
 *     ...
 *     
 *     readonly UnionTag __tag;
 *     __UnmanagedUnion __unmanageds;
 *     
 *     // 外部库可能会修改struct结构，所以即使字段全是引用类型，managed struct也不能直接映射到objects
 *     ManagedA __managed0;
 *     ManagedB __managed1;
 *     ...
 *     
 *     #pragma warning disable CS8618
 *     private Union(UnionTag tag) => __tag = tag;
 *     
 *     public UnionTag Tag => __tag;
 *     
 *     [UnscopedRef]
 *     public VariantVariant Variant => new VariantVariant(ref this);
 *     
 *     // accessibility可设置
 *     public Union CreateVariant(field) => new(UnionTag.Variant) { __field = field, };
 *     
 *     public bool TryGetVariant(out field)
 *     {
 *         if (__tag is UnionTag.Variant) {
 *             field = _field;
 *             return true;
 *         }
 *         else {
 *             field = default!;
 *             return false;
 *         }
 *     }
 * 
 *     struct __UnmanagedUnion { } // 对齐所有Unmanaged struct
 *     
 *     ref struct VariantVariant(ref Union union)
 *     {
 *         private ref Union _unionRef = ref union;
 *         
 *         public ref Field => _unionRef._field;
 *         
 *         // 异步和迭代器中不能使用ref struct，因此提供Deconstruct很有用
 *         // 但是只有一个field时没用，此时不生成，用Field属性获取
 *         public void Deconstruct(out field) => field = _unionRef._field;
 *     }
 * }
 */

/**abstract partial class Union
 * {
 *     sealed partial class Variant(field) : Union
 *     {
 *         public Field; // yes we use public field rather than property
 *     }
 * }
 */

/**abstract record Union
 * {
 *     sealed record Variant(fields) : Union {}
 * }
 */

/**RefStruct
 * [tag|ref byte|ref byte|..]
 */

/** Struct的详细设计
 * int _maxRefTypeCount;
 * INamedTypeSymbol _tagSymbol;
 * //dict 使用草稿见Program.Partial.Do()
 * //managed struct的处理和unmanaged应该是一致的，两者的区别只有是不是union而已 
 * Dictionary<ITypeSymbol, string Identifier> _unmanagedStructs;
 * Dictionary<ITypeSymbol, string Identifier> _managedStructs;
 */

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Trarizon.Library.GeneratorToolkit;
using Trarizon.Library.GeneratorToolkit.Extensions;
using Trarizon.Library.GeneratorToolkit.Helpers;
using Trarizon.Library.GeneratorToolkit.More;
using Trarizon.Library.GeneratorToolkit.Wrappers;

namespace Trarizon.Library.SourceGenerator.Generators;
[Generator(LanguageNames.CSharp)]
internal sealed partial class TaggedUnionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var filter = context.SyntaxProvider.ForAttributeWithMetadataName(
            L_Attribute_TypeName,
            (node, token) => node is EnumDeclarationSyntax,
            Emitter.Parse)
            .OfNotNull();

        context.RegisterSourceOutput(filter);
    }

    private sealed class Emitter(EnumDeclarationSyntax syntax, INamedTypeSymbol symbol,
        string taggedUnionTypeIdentifier, IEnumerable<Variant> variants, int maxReferenceCount) : ISourceEmitter
    {
        public string GenerateFileName() => $"{symbol.ToValidFileNameString()}.g.{Literals.TaggedUnionGenerator_Id}.cs";

        public static ParseResult<Emitter> Parse(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            if (context.TargetNode is not EnumDeclarationSyntax syntax)
                return default;

            if (context.TargetSymbol is not INamedTypeSymbol { TypeKind: TypeKind.Enum } symbol)
                return default;

            if (context.Attributes is not [var attribute])
                return default;

            var generatedTypeIdentifier = attribute.GetConstructorArgument<string?>(L_Attribute_GeneratedTypeName_ConstructorIndex).Value;
            if (generatedTypeIdentifier is null) {
                generatedTypeIdentifier = GetDefaultGeneratedTypeIdentifier(symbol);
            }
            else if (!ValidationHelper.IsValidIdentifier(generatedTypeIdentifier))
                return new DiagnosticData(Literals.Diagnostic_InvalidIdentifier_0Identifiers, syntax.Identifier.GetLocation(), generatedTypeIdentifier);

            var variantResults = symbol.GetMembers()
                .OfType<IFieldSymbol>()
                // Collect TagVariant attribute datas
                .Select(Result<Variant, DiagnosticData> (field) =>
                {
                    var attr = field.GetAttributes()
                       .Where(attr => attr.AttributeClass?.ToDisplayString(MoreSymbolDisplayFormats.DefaultWithoutGeneric) is L_VariantAttribute_TypeName)
                       .FirstOrDefault();
                    if (attr is null)
                        return new Variant(field, ImmutableArray<ITypeSymbol>.Empty, ImmutableArray<string?>.Empty);

                    ImmutableArray<ITypeSymbol> types;
                    ImmutableArray<string?> identifiers;

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

                    var invalidIdentifiers = identifiers
                        .Where(id => id is not null && !ValidationHelper.IsValidIdentifier(id))
                        .ToListIfAny();
                    if (invalidIdentifiers is not null)
                        return new DiagnosticData(Literals.Diagnostic_InvalidIdentifier_0Identifiers,
                            field.DeclaringSyntaxReferences[0].GetSyntax().GetLocation(),
                            string.Join(", ", invalidIdentifiers));

                    return new Variant(field,
                        types.EmptyIfDefault(),
                        identifiers);
                });

            token.ThrowIfCancellationRequested();

            var unsupports = variantResults
                .Where(r => r.Failed)
                .Select(r => r.Error)
                .ToListIfAny();
            if (unsupports is not null)
                return unsupports;

            // Distinct enum values
            var duplicateEnumValues = variantResults
                .DuplicatesBy(v => v.Value.EnumValue)
                .Select(v => new DiagnosticData(D_EnumValueRepeat, v.Value.EnumField.DeclaringSyntaxReferences[0]))
                .ToListIfAny();
            if (duplicateEnumValues is not null)
                return duplicateEnumValues;

            // Distinct identifiers
            duplicateEnumValues = variantResults
                .Where(v => !v.Value.Fields.IsDistinctBy(f => f.Identifier))
                .Select(v => new DiagnosticData(D_VariantFieldNameRepeat, v.Value.EnumField.DeclaringSyntaxReferences[0]))
                .ToListIfAny();
            if (duplicateEnumValues is not null)
                return duplicateEnumValues;

            token.ThrowIfCancellationRequested();

            var variants = variantResults.Select(r => r.Value).ToList();
            int max = variants.Max(v => v.ReferenceCount);
            var res = new ParseResult<Emitter>(new Emitter(syntax, symbol, generatedTypeIdentifier, variants, max));

            foreach (var variant in variants) {
                if (variant.Fields.Any(f => f.Type is { IsReferenceType: false, IsUnmanagedType: false }))
                    res.AddDiagnostic(new DiagnosticData(D_ManagedValueTypeWillBeBoxes, variant.EnumField.DeclaringSyntaxReferences[0]));
            }
            if (!variants.Any(v => v is { EnumValue: 0L, Fields.Length: 0 }))
                res.AddDiagnostic(new DiagnosticData(D_ProvideNonVariantZeroField, syntax.Identifier.GetLocation()));
            return res;
        }

        private static string GetDefaultGeneratedTypeIdentifier(INamedTypeSymbol symbol)
        {
            var enumName = symbol.Name;
            if (enumName.EndsWith("Kind") && enumName.Length > 4)
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
                EmitXmlDocComments(writer);
                using (writer.WriteLineWithBrace($"{syntax.Modifiers} partial struct {taggedUnionTypeIdentifier}")) {
                    writer.WriteLine(Literals.GeneratedCodeAttributeList_Code);
                    writer.WriteLine($"public readonly {symbol.ToFullQualifiedDisplayString()} {L_Tag_PropertyIdentifier} {{ get; private init; }}");

                    bool containsUnmanaged = variants.Any(v => v.UnmanagedCount > 0);
                    if (containsUnmanaged) {
                        writer.WriteLine(Literals.GeneratedCodeAttributeList_Code);
                        writer.WriteLine($"private {L_UnionStruct_TypeIdentifier} {L_UnionStruct_FieldIdentifier};");
                    }
                    EmitRefObjFields(writer);

                    foreach (var variant in variants) {
                        writer.WriteLine();
                        EmitCreateMethod(writer, variant);
                        writer.WriteLine();
                        EmitTryGetMethod(writer, variant);
                    }

                    if (containsUnmanaged) {
                        writer.WriteLine();
                        EmitUnionStruct(writer);
                    }
                }
            }


            return sw.ToString();
        }

        public void EmitXmlDocComments(IndentedTextWriter writer)
        {
            writer.WriteXmlDocLine("<remarks>", noEscape: true);
            writer.WriteXmlDocLine("<code>", noEscape: true);
            writer.WriteXmlDocLine($"union {taggedUnionTypeIdentifier}");
            writer.WriteXmlDocLine($"{{");
            foreach (var variant in variants) {
                writer.WriteXmlDocLine($"    {variant.EnumField.Name}({string.Join(", ", variant.Fields.Select(f => $"{f.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} {f.Identifier}"))}),");
            }
            writer.WriteXmlDocLine($"}}");
            writer.WriteXmlDocLine("</code>", noEscape: true);
            writer.WriteXmlDocLine("</remarks>", noEscape: true);
        }

        private void EmitRefObjFields(IndentedTextWriter writer)
        {
            for (int i = 0; i < maxReferenceCount; i++) {
                writer.WriteLine(Literals.GeneratedCodeAttributeList_Code);
                writer.WriteLine($"private object {L_RefObj_FieldIdentifier(i)};");
            }
        }

        private void EmitCreateMethod(IndentedTextWriter writer, Variant variant)
        {
            string parameters = string.Join(", ", variant.Fields.Select(f => $"{f.Type.ToFullQualifiedDisplayString()} {f.Identifier}"));
            using (writer.WriteLineWithIndent($"public static {taggedUnionTypeIdentifier} Create{variant.EnumField.Name}({parameters}) => new()", "{", "};")) {
                // initializer list
                writer.WriteLine($"{L_Tag_PropertyIdentifier} = {symbol.ToFullQualifiedDisplayString()}.{variant.EnumField.Name},");
                if (variant.Fields.Length == 0)
                    return;

                foreach (var field in variant.Fields.Where(f => !f.Type.IsUnmanagedType)) {
                    writer.WriteLine($"{L_RefObj_FieldIdentifier(field.FieldIndex)} = {field.Identifier},");
                }

                if (variant.UnmanagedCount == 0)
                    return;
                var unmanageds = string.Join(", ", variant.Fields.Where(f => f.Type.IsUnmanagedType).Select(f => f.Identifier));
                // We always use () wrap field, If there's only 1 field,
                // this () will be treat as redundant by compiler.
                writer.WriteLine($"{L_UnionStruct_FieldIdentifier} = new() {{ {variant.EnumField.Name} = ({unmanageds}), }},");
            }
        }

        private void EmitTryGetMethod(IndentedTextWriter writer, Variant variant)
        {
            if (variant.Fields.IsEmpty)
                return;

            IEnumerable<string> parameters = variant.Fields
                .Select((f) =>
                {
                    var attr = f.Type.IsReferenceType ? $"[{Globals.MaybeNullWhenAttribute_TypeName}(false)] " : "";
                    return $"{attr}out {f.Type.ToFullQualifiedDisplayString()} {f.Identifier}";
                });

            using (writer.WriteLineWithBrace($"public readonly bool TryGet{variant.EnumField.Name}({string.Join(", ", parameters)})")) {
                using (writer.WriteLineWithBrace($"if ({L_Tag_PropertyIdentifier} is {symbol.ToFullQualifiedDisplayString()}.{variant.EnumField.Name})")) {
                    Func<string, int, string> unmanagedAssign = variant.UnmanagedCount == 1
                        ? (id, i) => $"{id} = {L_UnionStruct_FieldIdentifier}.{variant.EnumField.Name};"
                        : (id, i) => $"{id} = {L_UnionStruct_FieldIdentifier}.{variant.EnumField.Name}.Item{i + 1};";

                    foreach (var (type, identifier, fieldIndex) in variant.Fields) {
                        if (type.IsUnmanagedType)
                            writer.WriteLine(unmanagedAssign(identifier, fieldIndex));
                        else if (type.IsReferenceType)
                            writer.WriteLine($"{identifier} = global::{Globals.Unsafe_TypeName}.{Globals.As_Identifier}<{type.ToFullQualifiedDisplayString()}>({L_RefObj_FieldIdentifier(fieldIndex)});");
                        else
                            writer.WriteLine($"{identifier} = global::{Globals.Unsafe_TypeName}.{Globals.Unbox_Identifier}<{type.ToFullQualifiedDisplayString()}>({L_RefObj_FieldIdentifier(fieldIndex)});");
                    }
                    writer.WriteLine("return true;");
                }
                foreach (var field in variant.Fields) {
                    writer.WriteLine($"{field.Identifier} = default;");
                }
                writer.WriteLine("return false;");
            }
        }

        private void EmitUnionStruct(IndentedTextWriter writer)
        {
            // Union unmanaged types

            writer.WriteLine(Literals.GeneratedCodeAttributeList_Code);
            writer.WriteLine($"[global::{Globals.StructLayoutAttribute_TypeName}(global::{Globals.LayoutKind_Explicit_EnumValue})]");
            using (writer.WriteLineWithBrace($"private struct {L_UnionStruct_TypeIdentifier}")) {
                // We do not generate type for simple enum field with no variant
                foreach (var variant in variants.Where(v => v.UnmanagedCount > 0)) {
                    writer.WriteLine($"[global::{Globals.FieldOffsetAttribute_TypeName}(0)]");

                    string typeName;
                    if (variant.UnmanagedCount == 1)
                        typeName = variant.Fields[0].Type.ToFullQualifiedDisplayString();
                    else {
                        var types = variant.Fields
                            .Where(f => f.Type.IsUnmanagedType)
                            .Select(f => f.Type.ToFullQualifiedDisplayString());
                        typeName = $"({string.Join(", ", types)})";
                    }
                    writer.WriteLine($"public {typeName} {variant.EnumField.Name};");
                }
            }
        }
    }

    private sealed class Variant
    {
        public IFieldSymbol EnumField { get; }

        public long EnumValue { get; }

        public ImmutableArray<(ITypeSymbol Type, string Identifier, int FieldIndex)> Fields { get; }

        public int ReferenceCount => Fields.Length - UnmanagedCount;
        public int UnmanagedCount { get; }

        public Variant(IFieldSymbol enumField, ImmutableArray<ITypeSymbol> types, ImmutableArray<string?> identifiers)
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

            var builder = ImmutableArray.CreateBuilder<(ITypeSymbol, string, int)>(types.Length);
            int refCount = 0, unmCount = 0;
            for (int i = 0; i < types.Length; i++) {
                var type = types[i];
                var id = identifiers.TryAt(i).Value ?? $"Item{i + 1}";
                ref var index = ref (type.IsUnmanagedType ? ref unmCount : ref refCount);
                builder.Add((type, id, index));
                index++;
            }
            Fields = builder.ToImmutable();
            UnmanagedCount = unmCount;
        }
    }
}
