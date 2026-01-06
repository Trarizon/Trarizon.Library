using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Trarizon.Library.Linq;
using Trarizon.Library.Roslyn;
using Trarizon.Library.Roslyn.Collections;
using Trarizon.Library.Roslyn.Emitting;
using Trarizon.Library.Roslyn.Extensions;
using Trarizon.Library.Roslyn.SourceInfos;
using Trarizon.Library.Roslyn.SourceInfos.CSharp;
using Trarizon.Library.Roslyn.SourceInfos.Emitting;

namespace Trarizon.Library.Functional.SourceGeneration;

[Generator(LanguageNames.CSharp)]
internal sealed class UnionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var hasMaybeNull = context.CompilationProvider.Select((compilation, token) =>
        {
            if (compilation.TryGetTypeByMetadataName("System.Diagnostics.CodeAnalysis.MaybeNullWhenAttribute", out _))
                return true;
            return false;
        });

        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Trarizon.Library.Functional.Unions.Attributes.UnionAttribute",
            (node, token) => node is StructDeclarationSyntax,
            (context, token) =>
            {
                if (context.TargetNode is not StructDeclarationSyntax syntax)
                    return null;
                if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
                    return null;
                if (context.Attributes is not [var attr])
                    return null;
                var variantTypes = attr.GetConstructorArgument(0).CastArray<ITypeSymbol>();

                bool hasReferenceType = false;

                List<VariantData> datas = [];
                int unmanagedIdx = 0;
                var unmanagedMap = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);
                int managedIdx = 0;
                var managedMap = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);

                foreach (var type in variantTypes) {
                    VariantTypeKind typeKind;
                    bool isInterface;
                    string fieldName;
                    FieldToPropertyConvert convert;
                    if (type.IsReferenceType) {
                        hasReferenceType = true;
                        typeKind = VariantTypeKind.Reference;
                        isInterface = type.TypeKind is TypeKind.Interface;
                        fieldName = "__obj";
                        convert = FieldToPropertyConvert.UnsafeAs;
                    }
                    else if (type.IsUnmanagedType) {
                        typeKind = VariantTypeKind.Unmanaged;
                        if (!unmanagedMap.TryGetValue(type, out var idx)) {
                            idx = unmanagedIdx++;
                            unmanagedMap.Add(type, idx);
                        }
                        isInterface = false;
                        fieldName = $"__val{idx}";
                        convert = FieldToPropertyConvert.Direct;
                    }
                    else {
                        typeKind = VariantTypeKind.Managed;
                        if (!managedMap.TryGetValue(type, out var idx)) {
                            idx = managedIdx++;
                            managedMap.Add(type, idx);
                        }
                        isInterface = false;
                        fieldName = $"__managed{idx}";
                        convert = FieldToPropertyConvert.Direct;
                    }

                    datas.Add(new VariantData(
                        datas.Count + 1, // 0 is for null
                        type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        typeKind, isInterface, fieldName, convert));
                }

                var fields = new List<(string, string)>();
                if (hasReferenceType)
                    fields.Add(("object", "__obj"));
                fields.AddRange(managedMap.Select(kv => (kv.Key.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), $"__managed{kv.Value}")));
                if (unmanagedMap.Count > 0) {
                    fields.Add(("__UnmanagedUnionStruct", $"__unmanageds"));
                }

                return new Data(
                    EmittingHelpers.ToFileNameString(typeSymbol.ToDisplayString()),
                    TypeHierarchyInfo.Create(typeSymbol, syntax),
                    typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    datas.ToSequenceEquatableImmutableArray(),
                    fields.ToSequenceEquatableImmutableArray(),
                    unmanagedMap.Select(kv => (kv.Key.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), $"__val{kv.Value}")).ToSequenceEquatableImmutableArray());
            })
            .OfNotNull()
            .Where(data => data.Variants.Length > 0);

        context.RegisterSourceOutput(source.Combine(hasMaybeNull), (context, src) =>
        {
            var (data, hasMaybeNull) = src;
            using var sw = new StringWriter();
            using var writer = new IndentedTextWriter(sw);
            new Emitter(writer, data, hasMaybeNull).Emit();
            context.AddSource($"{data.FileHintName}.g.cs", sw.ToString());
        });
    }

    private readonly struct Emitter(IndentedTextWriter writer, Data data, bool hasMaybeNull)
    {
        public void Emit()
        {
            using (writer.EmitCSharpTypeHierarchy(data.TypeHierarchy, partial: true,
                beforeDefinationLine: (writer, info) => writer.WriteLine(Codes.GeneratedCodeAttributeList))) {
                EmitFields();
                writer.WriteLine();
                EmitConstructors();
                EmitImplicitCastOperators();
                EmitIsNullProperty();
                writer.WriteLine();
                EmitTryAsMethod();
                writer.WriteLine();
                EmitAsMethod();
                writer.WriteLine();
                EmitIsMethod();
                writer.WriteLine();
                EmitUnmanagedUnionStruct();
            }
        }

        private void EmitFields()
        {
            writer.WriteLine($"private readonly int __flag;");
            foreach (var field in data.Fields) {
                writer.WriteLine($"private readonly {field.Type} {field.Name};");
            }
        }

        private void EmitConstructors()
        {
            foreach (var variant in data.Variants) {
                writer.WriteLine(Codes.GeneratedCodeAttributeList);
                writer.WriteMultipleLines($$"""
                    public {{data.TypeHierarchy.Name}}({{variant.FullyQualifiedTypeName}} value)
                    {
                        this.__flag = {{variant.Id}};
                        {{VariantAccessExpr(variant)}} = value;
                    }
                    """);
                writer.WriteLine();
            }
        }

        private void EmitImplicitCastOperators()
        {
            foreach (var variant in data.Variants) {
                writer.WriteLine(Codes.GeneratedCodeAttributeList);
                writer.WriteMultipleLines($$"""
                    public static implicit operator {{data.FullyQualifiedTypeName}}({{variant.FullyQualifiedTypeName}} value)
                    {
                        new {{data.FullyQualifiedTypeName}}(value);
                    }
                    """);
                writer.WriteLine();
            }
        }

        private void EmitIsNullProperty()
        {
            writer.WriteLine(Codes.GeneratedCodeAttributeList);
            writer.WriteMultipleLines($$"""
                public bool IsNull
                {
                    get {
                        return __flag == 0;
                    }
                }
                """);
        }

        private void EmitTryAsMethod()
        {
            writer.WriteLine(Codes.GeneratedCodeAttributeList);
            writer.WriteLine($"public bool TryAs<T>({(hasMaybeNull ? "[global::System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] " : "")}out T value)");
            using (writer.EnterBracketIndentScope('{')) {
                foreach (var variant in data.Variants) {
                    writer.WriteMultipleLines($$"""
                        if (typeof(T) == typeof({{variant.FullyQualifiedTypeName}}))
                        {
                            if (__flag != {{variant.Id}})
                            {
                                value = default;
                                return false;
                            }
                            value = {{VariantToTExpr(variant)}};
                            return true;
                        }
                        """);
                }
                writer.WriteLine("value = default;");
                writer.WriteLine($"return false;");
            }
        }

        private void EmitAsMethod()
        {
            writer.WriteLine(Codes.GeneratedCodeAttributeList);
            writer.WriteLine($"public T? As<T>()");
            using (writer.EnterBracketIndentScope('{')) {
                foreach (var variant in data.Variants) {
                    writer.WriteMultipleLines($$"""
                        if (typeof(T) == typeof({{variant.FullyQualifiedTypeName}}))
                        {
                            if (__flag != {{variant.Id}})
                                return default!;
                            return {{VariantToTExpr(variant)}};
                        }
                        """);
                }
                writer.WriteLine($"return default;");
            }
        }

        private void EmitIsMethod()
        {
            writer.WriteLine(Codes.GeneratedCodeAttributeList);
            writer.WriteLine($"public bool Is<T>()");
            using (writer.EnterBracketIndentScope('{')) {
                foreach (var variant in data.Variants) {
                    writer.WriteLine($"if (typeof(T) == typeof({variant.FullyQualifiedTypeName}))");
                    writer.WriteLine($"    return __flag == {variant.Id};");
                }
                writer.WriteLine($"return false;");
            }
        }

        private void EmitUnmanagedUnionStruct()
        {
            if (data.UnmanagedUnionFields.Length > 0) {
                writer.WriteLine($"[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]");
                writer.WriteLine(Codes.GeneratedCodeAttributeList);
                writer.WriteLine($"private readonly struct __UnmanagedUnionStruct");
                using (writer.EnterBracketIndentScope('{')) {
                    foreach (var field in data.UnmanagedUnionFields) {
                        writer.WriteLine($"[global::System.Runtime.InteropServices.FieldOffset(0)]");
                        writer.WriteLine($"public readonly {field.Type} {field.Name};");
                    }
                }
            }
        }
    }

    static string VariantAccessExpr(VariantData variant)
    {
        return variant.TypeKind is VariantTypeKind.Unmanaged
            ? $"this.__unmanageds.{variant.FieldName}" : $"this.{variant.FieldName}";
    }

    static string VariantToTExpr(VariantData variant)
    {
        var fieldFullyQualifiedTypeName = variant.TypeKind is VariantTypeKind.Reference
            ? "object" : variant.FullyQualifiedTypeName;
        var convert = $"global::System.Runtime.CompilerServices.Unsafe.As<{fieldFullyQualifiedTypeName}, T>(ref global::System.Runtime.CompilerServices.Unsafe.AsRef<{fieldFullyQualifiedTypeName}>(in {VariantAccessExpr(variant)}))";
        return convert;
    }

    sealed record class Data(
        string FileHintName,
        TypeHierarchyInfo TypeHierarchy,
        string FullyQualifiedTypeName,
        SequenceEquatableImmutableArray<VariantData> Variants,
        SequenceEquatableImmutableArray<(string Type, string Name)> Fields,
        SequenceEquatableImmutableArray<(string Type, string Name)> UnmanagedUnionFields);

    sealed record class VariantData(
        int Id,
        string FullyQualifiedTypeName,
        VariantTypeKind TypeKind,
        bool IsInterface,
        string FieldName,
        FieldToPropertyConvert Convert);

    enum VariantTypeKind { Reference, Managed, Unmanaged }

    enum FieldToPropertyConvert { Direct, UnsafeAs }
}
