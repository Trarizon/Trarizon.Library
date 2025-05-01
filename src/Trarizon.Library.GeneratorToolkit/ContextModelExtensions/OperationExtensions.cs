using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Trarizon.Library.Collections;

namespace Trarizon.Library.GeneratorToolkit.ContextModelExtensions;
public static class OperationExtensions
{
    public static IEnumerable<IOperation> Ancestors(this IOperation operation, bool includeSelf = false)
        => TraEnumerable.EnumerateByNotNull(includeSelf ? operation : operation.Parent, o => o.Parent);

    public static (string FilePath, int Line, int Column) GetInterceptedLocation(this IInvocationOperation operation)
    {
        var invocationExprSyntax = (InvocationExpressionSyntax)operation.Syntax;
        var memberAccessExprSyntax = (MemberAccessExpressionSyntax)invocationExprSyntax.Expression;
        var invocationNameSpan = memberAccessExprSyntax.Name.Span;
        var syntaxTree = invocationExprSyntax.SyntaxTree;

        var lineSpan = syntaxTree.GetLineSpan(invocationNameSpan);
        var sourceRefResolver = operation.SemanticModel?.Compilation.Options.SourceReferenceResolver;
        var filePath = sourceRefResolver?.NormalizePath(syntaxTree.FilePath, null) ?? syntaxTree.FilePath;
        return (filePath, lineSpan.StartLinePosition.Line + 1, lineSpan.StartLinePosition.Character + 1);
    }
}
