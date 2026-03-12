using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Schema;
using Trarizon.Library.Linq;
using Trarizon.Library.Roslyn;
using Trarizon.Library.Roslyn.Emitting;
using Trarizon.Library.Roslyn.Extensions;
using Trarizon.Library.Roslyn.SourceInfos;
using Trarizon.Library.Roslyn.SourceInfos.CSharp;
using Trarizon.Library.Roslyn.SourceInfos.Emitting;

namespace Trarizon.Library.Functional.SourceGeneration.TypeUnion;

[Generator(LanguageNames.CSharp)]
internal sealed partial class TypeUnionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var hasMaybeNull = context.CompilationProvider.Select((compilation, token) =>
        {
            if (compilation.TryGetTypeByMetadataName("System.Diagnostics.CodeAnalysis.MaybeNullWhenAttribute", out _))
                return true;
            return false;
        });

        const string AttributeTypeName = "Trarizon.Library.Functional.Attributes.TypeUnionAttribute";

        // Non-generic ver.
        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeTypeName,
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

                return ParseCore(new ParseContext(syntax, typeSymbol, attr, variantTypes), token);
            })
            .OfNotNull()
            .Where(data => data.Variants.Length > 0);

        context.RegisterSourceOutput(source.Combine(hasMaybeNull), (context, src) =>
        {
            var (data, hasMaybeNull) = src;
            using var sw = new StringWriter();
            using var writer = new IndentedTextWriter(sw);
            new UnionEmitter(writer, data, hasMaybeNull).Emit();
            context.AddSource($"{data.FileHintName}.g.cs", sw.ToString());
        });

        // Generic ver.
        for (int i = 2; i < 3; i++) {
            var sourceg = context.SyntaxProvider.ForAttributeWithMetadataName(
                $"{AttributeTypeName}`{i}",
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

                    return ParseCore(new ParseContext(syntax, typeSymbol, attr, variantTypes), token);
                })
                .OfNotNull()
                .Where(data => data.Variants.Length > 0);

            context.RegisterSourceOutput(sourceg.Combine(hasMaybeNull), (context, src) =>
            {
                var (data, hasMaybeNull) = src;
                using var sw = new StringWriter();
                using var writer = new IndentedTextWriter(sw);
                new UnionEmitter(writer, data, hasMaybeNull).Emit();
                context.AddSource($"{data.FileHintName}.g.cs", sw.ToString());
            });
        }
    }

    private UnionData? ParseCore(ParseContext ctx, CancellationToken cancellationToken)
    {
        var (syntax, typeSymbol, attr, variantTypes) = ctx;

        if (variantTypes.Length == 0)
            return null;

        List<VariantData> datas = [];
        int unmanagedIdx = 0;
        var unmanagedMap = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);
        int managedIdx = 0;
        var managedMap = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);

        foreach (var (typeIndex, type) in variantTypes.Index()) {
            var id = typeIndex + 1;
            var fqname = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            VariantData data;
            if (type.IsReferenceType) {
                data = new VariantData(id, fqname, VariantTypeKind.Reference)
                {
                    IsInterface = type.TypeKind is TypeKind.Interface
                };
            }
            else if (type.IsUnmanagedType) {
                if (!unmanagedMap.TryGetValue(type, out var idx)) {
                    idx = unmanagedIdx++;
                    unmanagedMap.Add(type, idx);
                }
                data = new VariantData(id, fqname, VariantTypeKind.Unmanaged)
                {
                    FieldId = idx,
                    IsRefLikeType = type.IsRefLikeType
                };
            }
            else {
                if (!managedMap.TryGetValue(type, out var idx)) {
                    idx = managedIdx++;
                    managedMap.Add(type, idx);
                }
                data = new VariantData(id, fqname, VariantTypeKind.Managed)
                {
                    FieldId = idx,
                    IsRefLikeType = type.IsRefLikeType
                };
            }

            datas.Add(data);
        }

        var shareInterface = attr.GetNamedArgument("ShareInterface").GetConstantValueOrDefault<ShareInterfaceOption>();
        IEnumerable<ITypeSymbol>? sharedInterfaces = null;
        if (shareInterface is not ShareInterfaceOption.Disabled) {
            sharedInterfaces = variantTypes
                .Select(x => x.AllInterfaces.AsEnumerable().Prepend(x)!)
                .Aggregate((l, r) => l.Intersect(r, (IEqualityComparer<ITypeSymbol>)SymbolEqualityComparer.Default));
        }

        var sharedIntfDatas = sharedInterfaces?.Select(x =>
        {
            var fqname = x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return (fqname, x.GetMembers()
                .Where(x =>
                {
                    if (x.IsImplicitlyDeclared)
                        return false;
                    if (x is IMethodSymbol m)
                        return m.MethodKind is MethodKind.Ordinary;
                    return true;
                })
                .Select(x =>
                {
                    if (x is IPropertySymbol prop) {
                        if (prop.IsIndexer) {
                            return new InterfaceMemberInfo(
                                InterfaceMemberKind.Indexer,
                                prop.IsStatic,
                                $"{(prop.ReturnsByRefReadonly ? "ref readonly " : prop.ReturnsByRef ? "ref " : "")}{prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedWithNullableAnnotation)}",
                                "this")
                            {
                                HasGetOrAddAccessor = prop.GetMethod is not null,
                                HasSetOrRemoveAccessor = prop.SetMethod is not null,
                                Parameters = prop.Parameters.Select(ParameterInfoSelector).ToSequenceEquatableImmutableArray(),
                                ReturnRefKind = prop.ReturnsByRefReadonly ? RefKind.RefReadOnly : prop.ReturnsByRef ? RefKind.Ref : RefKind.None,
                            };
                        }
                        else {
                            return new InterfaceMemberInfo(
                                InterfaceMemberKind.Property,
                                prop.IsStatic,
                                $"{(prop.ReturnsByRefReadonly ? "ref readonly " : prop.ReturnsByRef ? "ref " : "")}{prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedWithNullableAnnotation)}",
                                prop.Name)
                            {
                                HasGetOrAddAccessor = prop.GetMethod is not null,
                                HasSetOrRemoveAccessor = prop.SetMethod is not null,
                                ReturnRefKind = prop.ReturnsByRefReadonly ? RefKind.RefReadOnly : prop.ReturnsByRef ? RefKind.Ref : RefKind.None
                            };
                        }
                    }
                    if (x is IEventSymbol ev) {
                        return new InterfaceMemberInfo(
                            InterfaceMemberKind.Event,
                            ev.IsStatic,
                            ev.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedWithNullableAnnotation),
                            ev.Name)
                        {
                            HasGetOrAddAccessor = ev.AddMethod is not null,
                            HasSetOrRemoveAccessor = ev.RemoveMethod is not null
                        };
                    }
                    if (x is IMethodSymbol m) {
                        return new InterfaceMemberInfo(
                            InterfaceMemberKind.Method,
                            m.IsStatic,
                            m.ReturnsVoid ? "void"
                                : $"{(m.ReturnsByRefReadonly ? "ref readonly " : m.ReturnsByRef ? "ref " : "")}{m.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedWithNullableAnnotation)}",
                            m.Name)
                        {
                            TypeParameters = m.TypeParameters.Select(x => x.Name).ToSequenceEquatableImmutableArray(),
                            Parameters = m.Parameters.Select(ParameterInfoSelector).ToSequenceEquatableImmutableArray(),
                            Constraints = string.Join(" ", m.TypeParameters.Select(CodeFactory.GetConstraintText)),
                            ReturnsVoid = m.ReturnsVoid,
                            ReturnRefKind = m.ReturnsByRefReadonly ? RefKind.RefReadOnly : m.ReturnsByRef ? RefKind.Ref : RefKind.None
                        };
                    }
                    return default;

                    static ParameterInfo ParameterInfoSelector(IParameterSymbol x)
                    {
                        var scoped = x.ScopedKind is ScopedKind.None ? "" : "scoped ";
                        var @ref = x.RefKind switch
                        {
                            RefKind.Ref => "ref ",
                            RefKind.Out => "out ",
                            RefKind.In => "in ",
                            RefKind.RefReadOnlyParameter => "ref readonly ",
                            _ => "",
                        };
                        return new ParameterInfo(
                            x.RefKind,
                            $"{scoped}{@ref}{x.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedWithNullableAnnotation)}",
                            x.Name);
                    }
                }).ToSequenceEquatableImmutableArray());
        });

        return new UnionData(
            EmittingHelpers.ToFileNameString(typeSymbol.ToDisplayString()),
            TypeHierarchyInfo.Create(typeSymbol, syntax),
            typeSymbol.Name,
            typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            ShareInterfaceOption.Explicit,//shareInterface,
            sharedIntfDatas?.ToSequenceEquatableImmutableArray() ?? new([]),
            datas.ToSequenceEquatableImmutableArray());
    }

    private class UnionEmitter(IndentedTextWriter writer, UnionData data, bool hasMaybeNull)
    {
        public void Emit()
        {
            var refLike = data.Variants.Any(v => v.IsRefLikeType) ? "ref " : "";

            writer.EmitTopTrivias(true, true);
            writer.WriteLine(CodeFactory.PragmaWarningTrivia(false, "CS8618"));

            using (writer.EmitCSharpTypeHierarchy(data.TypeHierarchy, partial: true,
                (writer, info) =>
                {
                    writer.WriteLine(Codes.GeneratedCodeAttributeList);
                    writer.WriteLine($"{refLike}partial {info.Keywords} {info.Name}");
                    if (data.Interfaces.Any()) {
                        writer.WriteLine($"    : {string.Join(", ", data.Interfaces.Select(x => x.FullQualifiedTypeName))}");
                    }
                })) {
                EmitFields();
                writer.WriteLine();
                EmitConstructors();
                writer.WriteLine();
                EmitImplicitCastOperators();
                writer.WriteLine();
                EmitIsNullProperty();
                writer.WriteLine();
                EmitTryAsMethod();
                writer.WriteLine();
                EmitAsMethod();
                writer.WriteLine();
                EmitIsMethod();
                if (data.Interfaces.Any()) {
                    writer.WriteLine();
                    EmitSharedInterfaceMethods();
                }
                if (data.Variants.Any(x => x.TypeKind is VariantTypeKind.Unmanaged)) {
                    writer.WriteLine();
                    EmitUnmanagedUnionStruct();
                }
            }
        }

        private void EmitFields()
        {
            writer.WriteLine($"private readonly int __flag;");
            bool hasReference = data.Variants.Any(x => x.TypeKind is VariantTypeKind.Reference);
            if (hasReference) {
                writer.WriteLine($"private readonly object __obj;");
            }
            foreach (var type in data.Variants.Where(x => x.TypeKind is VariantTypeKind.Managed)) {
                writer.WriteLine($"private readonly {type.FullyQualifiedTypeName} __managed{type.FieldId};");
            }
            if (data.Variants.Any(x => x.TypeKind is VariantTypeKind.Unmanaged)) {
                writer.WriteLine($"private readonly __UnmanagedUnionStruct __unmanageds;");
            }
        }

        private void EmitConstructors()
        {
            foreach (var variant in data.Variants.JoinWriteEmptyLine(writer)) {
                writer.WriteLine(Codes.GeneratedCodeAttributeList);
                writer.WriteMultipleLines($$"""
                    public {{data.TypeName}}({{variant.FullyQualifiedTypeName}} value)
                    {
                        this.__flag = {{variant.Id}};
                        {{VariantAccessExpr(variant)}} = value;
                    }
                    """);
            }
        }

        private void EmitImplicitCastOperators()
        {
            foreach (var variant in data.Variants.JoinWriteEmptyLine(writer)) {
                if (variant.IsInterface)
                    continue;

                writer.WriteLine(Codes.GeneratedCodeAttributeList);
                writer.WriteMultipleLines($$"""
                    public static implicit operator {{data.FullyQualifiedTypeName}}({{variant.FullyQualifiedTypeName}} value)
                    {
                        return new {{data.FullyQualifiedTypeName}}(value);
                    }
                    """);
            }
        }

        private void EmitIsNullProperty()
        {
            writer.WriteLine(Codes.GeneratedCodeAttributeList);
            writer.WriteLine("public bool IsNull { get { return this.__flag == 0; } }");
        }

        private void EmitTryAsMethod()
        {
            var maybeNull = hasMaybeNull ? "[global::System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] " : "";

            writer.WriteLine(Codes.GeneratedCodeAttributeList);

            writer.WriteLine($"public bool TryAs<T>({maybeNull}out T value)");
            using (writer.EnterBracketIndentScope('{')) {
                foreach (var variant in data.Variants) {
                    writer.WriteMultipleLines($$"""
                        if (typeof(T) == typeof({{variant.FullyQualifiedTypeName}}))
                        {
                            if (this.__flag == {{variant.Id}})
                            {
                                value = {{VariantToTExpr(variant)}};
                                return true;
                            }
                        }
                        """);
                }
                writer.WriteLine("value = default;");
                writer.WriteLine("return false;");
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
                            if (this.__flag == {{variant.Id}})
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
                    writer.WriteLine($"    return this.__flag == {variant.Id};");
                }
                writer.WriteLine("return false;");
            }
        }

        private void EmitSharedInterfaceMethods()
        {
            foreach (var intf in data.Interfaces.JoinWriteEmptyLine(writer)) {
                foreach (var m in intf.Methods.JoinWriteEmptyLine(writer)) {
                    writer.WriteLine(Codes.GeneratedCodeAttributeList);
                    switch (m.Kind) {
                        case InterfaceMemberKind.Property: {
                            writer.WriteLine(data.ShareInterface is ShareInterfaceOption.Explicit
                                ? $"{Optional.Create(m.IsStatic, "static ")}{m.ReturnTypeWithModifiers} {intf.FullQualifiedTypeName}.{m.Name}"
                                : $"public {Optional.Create(m.IsStatic, "static ")}{m.ReturnTypeWithModifiers} {m.Name}");
                            using (writer.EnterBracketIndentScope('{')) {
                                if (m.HasGetOrAddAccessor) {
                                    writer.WriteLine("get");
                                    using (writer.EnterBracketIndentScope('{')) {
                                        foreach (var variant in data.Variants) {
                                            writer.WriteLine($"if (this.__flag == {variant.Id})");
                                            writer.WriteLine($"    return {CodeFactory.GetReturnRefKeywords(m.ReturnRefKind, true)}{VariantToInterfaceExpr(variant, intf.FullQualifiedTypeName)}.{m.Name};");
                                        }
                                        writer.WriteLine($"return {CodeFactory.GetReturnRefKeywords(m.ReturnRefKind, true)}(({intf.FullQualifiedTypeName})null!).{m.Name};");
                                    }
                                }
                                if (m.HasSetOrRemoveAccessor) {
                                    writer.WriteLine("set");
                                    using (writer.EnterBracketIndentScope('{')) {
                                        foreach (var variant in data.Variants) {
                                            writer.WriteLine($"if (this.__flag == {variant.Id})");
                                            writer.WriteLine($"    {VariantToInterfaceExpr(variant, intf.FullQualifiedTypeName)}.{m.Name} = value;");
                                        }
                                        writer.WriteLine($"(({intf.FullQualifiedTypeName})null!).{m.Name} = value;");
                                    }
                                }
                            }
                            break;
                        }
                        case InterfaceMemberKind.Indexer: {
                            var parameters = string.Join(", ", m.Parameters.Select(x => $"{x.FullyQualifiedTypeNameWithModifiers} {x.Name}"));
                            var arguments = string.Join(", ", m.Parameters.Select(x => $"{CodeFactory.GetArgumentRefKeywords(x.RefKind, true)}{x.Name}"));
                            writer.WriteLine(data.ShareInterface is ShareInterfaceOption.Explicit
                                ? $"{Optional.Create(m.IsStatic, "static ")}{m.ReturnTypeWithModifiers} {intf.FullQualifiedTypeName}.{m.Name}[{parameters}]"
                                : $"public {Optional.Create(m.IsStatic, "static ")}{m.ReturnTypeWithModifiers} {m.Name}{parameters}");
                            using (writer.EnterBracketIndentScope('{')) {
                                if (m.HasGetOrAddAccessor) {
                                    writer.WriteLine("get");
                                    using (writer.EnterBracketIndentScope('{')) {
                                        foreach (var variant in data.Variants) {
                                            writer.WriteLine($"if (this.__flag == {variant.Id})");
                                            writer.WriteLine($"    return {CodeFactory.GetReturnRefKeywords(m.ReturnRefKind, true)}{VariantToInterfaceExpr(variant, intf.FullQualifiedTypeName)}.{m.Name}[{arguments}];");
                                        }
                                        writer.WriteLine($"return {CodeFactory.GetReturnRefKeywords(m.ReturnRefKind, true)}(({intf.FullQualifiedTypeName})null!).{m.Name}[{arguments}];");
                                    }
                                }
                                if (m.HasSetOrRemoveAccessor) {
                                    writer.WriteLine("set");
                                    using (writer.EnterBracketIndentScope('{')) {
                                        foreach (var variant in data.Variants) {
                                            writer.WriteLine($"if (this.__flag == {variant.Id})");
                                            writer.WriteLine($"    {VariantToInterfaceExpr(variant, intf.FullQualifiedTypeName)}.{m.Name}[{arguments}] = value;");
                                        }
                                        writer.WriteLine($"(({intf.FullQualifiedTypeName})null!).{m.Name}[{arguments}] = value;");
                                    }
                                }
                            }
                            break;
                        }
                        case InterfaceMemberKind.Event: {
                            writer.WriteLine(data.ShareInterface is ShareInterfaceOption.Explicit
                                ? $"{Optional.Create(m.IsStatic, "static ")}event {m.ReturnTypeWithModifiers} {intf.FullQualifiedTypeName}.{m.Name}"
                                : $"public {Optional.Create(m.IsStatic, "static ")}event {m.ReturnTypeWithModifiers} {m.Name}");
                            using (writer.EnterBracketIndentScope('{')) {
                                if (m.HasGetOrAddAccessor) {
                                    writer.WriteLine("add");
                                    using (writer.EnterBracketIndentScope('{')) {
                                        foreach (var variant in data.Variants) {
                                            writer.WriteLine($"if (this.__flag == {variant.Id})");
                                            using (writer.EnterBracketIndentScope('{')) {
                                                writer.WriteLine($"{VariantToInterfaceExpr(variant, intf.FullQualifiedTypeName)}.{m.Name} += value;");
                                                writer.WriteLine($"return;");
                                            }
                                        }
                                        writer.WriteLine($"(({intf.FullQualifiedTypeName})null!).{m.Name} += value;");
                                    }
                                }
                                if (m.HasSetOrRemoveAccessor) {
                                    writer.WriteLine("remove");
                                    using (writer.EnterBracketIndentScope('{')) {
                                        foreach (var variant in data.Variants) {
                                            writer.WriteLine($"if (this.__flag == {variant.Id})");
                                            using (writer.EnterBracketIndentScope('{')) {
                                                writer.WriteLine($"{VariantToInterfaceExpr(variant, intf.FullQualifiedTypeName)}.{m.Name} -= value;");
                                                writer.WriteLine($"return;");
                                            }
                                        }
                                        writer.WriteLine($"(({intf.FullQualifiedTypeName})null!).{m.Name} -= value;");
                                    }
                                }
                            }
                            break;
                        }
                        case InterfaceMemberKind.Method: {
                            var parameters = string.Join(", ", m.Parameters.Select(x => $"{x.FullyQualifiedTypeNameWithModifiers} {x.Name}"));
                            var arguments = string.Join(", ", m.Parameters.Select(x => $"{CodeFactory.GetArgumentRefKeywords(x.RefKind, true)}{x.Name}"));
                            writer.WriteLine(data.ShareInterface is ShareInterfaceOption.Explicit
                                ? $"{Optional.Create(m.IsStatic, "static ")}{m.ReturnTypeWithModifiers} {intf.FullQualifiedTypeName}.{m.Name}({parameters}) {m.Constraints}"
                                : $"public {Optional.Create(m.IsStatic, "static ")}{m.ReturnTypeWithModifiers} {m.Name}({parameters}) {m.Constraints}");
                            using (writer.EnterBracketIndentScope('{')) {
                                foreach (var variant in data.Variants) {
                                    writer.WriteLine($"if (this.__flag == {variant.Id})");
                                    using (writer.EnterBracketIndentScope('{')) {
                                        if (m.ReturnsVoid) {
                                            writer.WriteLine($"{VariantToInterfaceExpr(variant, intf.FullQualifiedTypeName)}.{m.Name}({arguments});");
                                            writer.WriteLine($"return;");
                                        }
                                        else {
                                            writer.WriteLine($"return {CodeFactory.GetReturnRefKeywords(m.ReturnRefKind, true)}{VariantToInterfaceExpr(variant, intf.FullQualifiedTypeName)}.{m.Name}({arguments});");
                                        }
                                    }
                                }
                                if (m.ReturnsVoid) {
                                    writer.WriteLine($"(({intf.FullQualifiedTypeName})null!).{m.Name}({arguments});");
                                    writer.WriteLine($"return;");
                                }
                                else {
                                    writer.WriteLine($"return {CodeFactory.GetReturnRefKeywords(m.ReturnRefKind, true)}(({intf.FullQualifiedTypeName})null!).{m.Name}({arguments});");
                                }
                            }

                            break;
                        }
                        default:
                            break;
                    }
                }
            }
        }

        private void EmitUnmanagedUnionStruct()
        {
            writer.WriteLine($"[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]");
            writer.WriteLine(Codes.GeneratedCodeAttributeList);
            writer.WriteLine($"private struct __UnmanagedUnionStruct");
            using (writer.EnterBracketIndentScope('{')) {
                foreach (var variant in data.Variants.Where(x => x.TypeKind is VariantTypeKind.Unmanaged)) {
                    writer.WriteLine($"[global::System.Runtime.InteropServices.FieldOffset(0)]");
                    writer.WriteLine($"public {variant.FullyQualifiedTypeName} _{variant.FieldId};");
                }
            }
        }
    }

    static string VariantAccessExpr(VariantData variant)
    {
        return variant.TypeKind switch
        {
            VariantTypeKind.Reference => "this.__obj",
            VariantTypeKind.Managed => $"this.__managed{variant.FieldId}",
            VariantTypeKind.Unmanaged => $"this.__unmanageds._{variant.FieldId}",
            _ => null!,
        };
    }

    static string VariantToTExpr(VariantData variant, string targetType = "T")
    {
        var fieldFullyQualifiedTypeName = variant.TypeKind is VariantTypeKind.Reference
            ? "object" : variant.FullyQualifiedTypeName;
        var convert = $"global::System.Runtime.CompilerServices.Unsafe.As<{fieldFullyQualifiedTypeName}, {targetType}>(ref global::System.Runtime.CompilerServices.Unsafe.AsRef<{fieldFullyQualifiedTypeName}>(in {VariantAccessExpr(variant)}))";
        return convert;
    }

    static string VariantToInterfaceExpr(VariantData variant, string targetType)
    {
        switch (variant.TypeKind) {
            case VariantTypeKind.Reference:
                return $"global::System.Runtime.CompilerServices.Unsafe.As<object, {targetType}>(ref global::System.Runtime.CompilerServices.Unsafe.AsRef<object>(in {VariantAccessExpr(variant)}))";
            default:
                return $"(({targetType}){VariantAccessExpr(variant)})";
        }
    }
}
