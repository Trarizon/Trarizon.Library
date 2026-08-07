using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.CodeDom.Compiler;
using Trarizon.Library.Roslyn;

namespace Trarizon.Library.Functional.Generators.TypeUnion;

[Generator(LanguageNames.CSharp)]
public sealed partial class TypeUnionGenerator : IIncrementalGenerator
{
    const string TypeUnionAttrMName = "Trarizon.Library.Functional.Unions.TypeUnionAttribute";
    const string TypeUnionAttr2MName = "Trarizon.Library.Functional.Unions.TypeUnionAttribute`2";

    private record class Env(
        bool MaybeNull,
        bool UnscopedRef,
        string? TargetFramework,
        LanguageVersion LanguageVersion
    )
    {
        public bool AllowsRefStruct
        {
            get
            {
                if (TargetFramework is null)
                    return false;
                if (LanguageVersion < LanguageVersion.CSharp13)
                    return false;
                if (TargetFramework.StartsWith("net"))
                {
                    if (Version.TryParse(TargetFramework[3..], out var v))
                        return v.Major >= 9;
                }
                return false;
            }
        }
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var aco = context.AnalyzerConfigOptionsProvider.Select((provider, ct) =>
        {
            if (provider.GlobalOptions.TryGetValue("build_property.TargetFramework", out var targetFramework))
                return targetFramework;
            return null;
        });

        var po = context.ParseOptionsProvider.Select((options, ct) =>
        {
            var opts = (CSharpParseOptions)options;
            var lv = opts.LanguageVersion;
            return lv;
        });

        var apis = context.CompilationProvider.Select((compilation, ct) =>
        {
            bool maybeNull = compilation.TryGetTypeByMetadataName("System.Diagnostics.CodeAnalysis.MaybeNullWhenAttribute", out _);
            bool unscopedRef = compilation.TryGetTypeByMetadataName("System.Diagnostics.CodeAnalysis.UnscopedRefAttribute", out _);
            return (maybeNull, unscopedRef);
        });

        var env = aco.Combine(po).Combine(apis).Select(static (x, ct) =>
        {
            var ((tf, lv), (maybeNull, unscopedRef)) = x;
            return new Env(maybeNull, unscopedRef, tf, lv);
        });

        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            TypeUnionAttrMName,
            (node, ct) => node is StructDeclarationSyntax,
            Parse)
            .Where(x => x is not null);

        var source2 = context.SyntaxProvider.ForAttributeWithMetadataName(
            TypeUnionAttr2MName,
            (node, ct) => node is StructDeclarationSyntax,
            ParseGeneric);

        context.RegisterSourceOutput(source.Combine(env), (context, source) =>
        {
            try
            {
                if (source.Left is null)
                {
                    context.AddSource($"{id++}.g.cs", $$"""
                    // <generated>
                    partial struct A { public void Failure() { } }
                    /*
                    */
                    """);
                    return;
                }

                var data = source.Left;
                var compilation = source.Right;
                using var sw = new StringWriter();
                using var writer = new IndentedTextWriter(sw);
                EmitTypeUnion(writer, data, source.Right);
                context.AddSource(data.FileHintName, sw.ToString());
            }
            catch (Exception ex)
            {
                context.AddSource($"{id++}.g.cs", $$"""
                    // <generated>
                    partial struct A { public void Error() { } }
                    /*
                    {{ex.Message}}
                    {{ex.StackTrace}}
                    */
                    """);
            }
        });
    }
    static uint id;
}
