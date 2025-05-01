using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Trarizon.Library.Roslyn.Extensions;
public static class OperationExtensions
{
    public static IEnumerable<IOperation> Ancestors(this IOperation operation, bool includeSelf = false)
    {
        if (includeSelf)
            yield return operation;

        var op = operation.Parent;
        while (op is not null) {
            yield return op;
            op = op.Parent;
        }
    }
}
