﻿#if false

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using Trarizon.Library.GeneratorToolkit.ContextModelExtensions;
using Trarizon.Library.GeneratorToolkit.CSharp;
using static Trarizon.Library.SourceGeneration.Interceptors.EnumHasAnyFlagLiterals;

namespace Trarizon.Library.SourceGeneration.Interceptors;
// 不用Interceptor了，但是可以留着做个参考
//[Generator(LanguageNames.CSharp)]
internal class EnumHasAnyFlagInterceptor //: IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodsInvocations = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, token) =>
            {
                return node is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax
                    {
                        Name.Identifier.ValueText: L_HasAnyFlag_MethodIdentifier
                    }
                };
            },
            static (context, token) =>
            {
                var semanticModel = context.SemanticModel;
                var symbol = semanticModel.GetSymbolInfo(context.Node).Symbol as IMethodSymbol;

                if (!symbol!.ContainingType.MatchDisplayString(L_TraEnum_TypeName))
                    return null;

                var operation = context.SemanticModel.GetOperation(context.Node, token);
                if (operation is IInvocationOperation invocation) {
                    return invocation;
                }
                else {
                    return null;
                }
            })
            .OfNotNull()
            .Collect()
            .Select(static (invocations, token) =>
            {
                return invocations
                    .GroupBy(invocation => invocation.TargetMethod.TypeArguments[0], SymbolEqualityComparer.Default)
                    .Select(group =>
                    {
                        var type = group.Key!;
                        var attrs = group.Select(op =>
                        {
                            var (filePath, line, column) = op.GetInterceptorLocation();
                            return $"[{CodeFactory.InterceptsLocationAttribute(filePath, line, column)}]";
                        });
                        var defination = L_InterceptorMethodDeclaration(type.ToFullQualifiedDisplayString());
                        return (attrs, defination);
                    });
            });

        context.RegisterSourceOutput(methodsInvocations, (context, source) =>
        {
            if (!source.Any())
                return;

            var sw = new StringWriter();
            var writer = new IndentedTextWriter(sw);

            writer.WriteLine(Literals.AutoGenerated_TopTrivia_Code);
            writer.WriteLine();
            writer.EmitPragmaWarningTrivia("CS9113", false);
            writer.WriteLine();
            writer.WriteLine(CodeFactory.FileScopeInterceptsLocationAttributeDeclaration);
            writer.WriteLine();
            writer.EmitPragmaWarningTrivia("CS9113", true);
            writer.WriteLine();
            writer.WriteLine($"namespace {Literals.Generated_Namespace}");
            using (writer.WriteBracketIndentScope('{')) {
                writer.WriteLine(Literals.GeneratedCodeAttributeList_Code);
                writer.WriteLine($"internal static class {L_Interceptors_TypeName}");
                using (writer.WriteBracketIndentScope('{')) {
                    foreach (var (attrs, defination) in source) {
                        foreach (var attr in attrs)
                            writer.WriteLine(attr);
                        writer.WriteLine(defination);
                    }
                }
            }

            context.AddSource(L_FileName, sw.ToString());
        });
    }
}

#endif