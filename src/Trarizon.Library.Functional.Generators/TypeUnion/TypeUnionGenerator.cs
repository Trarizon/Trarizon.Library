using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.CodeDom.Compiler;
using Trarizon.Library.Roslyn;

namespace Trarizon.Library.Functional.Generators.TypeUnion;

[Generator(LanguageNames.CSharp)]
public sealed partial class TypeUnionGenerator : IIncrementalGenerator
{
    const string TypeUnionAttrMName = "Trarizon.Library.Functional.Attributes.TypeUnionAttribute";
    const string TypeUnionAttr2MName = "Trarizon.Library.Functional.Attributes.TypeUnionAttribute`2";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(context =>
        {
            context.AddSource($"__PointerHelpers.g.i.cs", GetPointeHelperDeclaration());
        });

        var env = context.CompilationProvider.Select((compilation, ct) =>
        {
            bool maybeNull = compilation.TryGetTypeByMetadataName("System.Diagnostics.CodeAnalysis.MaybeNullWhenAttribute", out _);
            bool unscopedRef = compilation.TryGetTypeByMetadataName("System.Diagnostics.CodeAnalysis.UnscopedRefAttribute", out _);
            return (maybeNull, unscopedRef);
        });

        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            TypeUnionAttrMName,
            (node, ct) => node is StructDeclarationSyntax,
            Parse);
        // .Where(x => x.HasValue);

        var source2 = context.SyntaxProvider.ForAttributeWithMetadataName(
            TypeUnionAttr2MName,
            (node, ct) => node is StructDeclarationSyntax,
            ParseGeneric);

        context.RegisterSourceOutput(source.Combine(env), (context, source) =>
        {
            if (!source.Left.HasValue)
            {
                context.AddSource($"{id++}.g.cs", "// <generated>");
                return;
            }

            var data = source.Left.Value;
            var compilation = source.Right;
            using var sw = new StringWriter();
            using var writer = new IndentedTextWriter(sw);
            EmitTypeUnion(writer, data, source.Right);
            context.AddSource(data.FileHintName, sw.ToString());
        });
    }
    static uint id;
}
