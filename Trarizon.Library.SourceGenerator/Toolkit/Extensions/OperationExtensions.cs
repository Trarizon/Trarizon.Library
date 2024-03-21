using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Trarizon.Library.SourceGenerator.Toolkit.Extensions;
internal static class OperationExtensions
{
    public static IEnumerable<IOperation> Ancestors(this IOperation operation, bool includeSelf = false)
    {
        if (includeSelf)
            yield return operation;

        while (true) {
            operation = operation.Parent!;
            if (operation is null)
                yield break;
            else
                yield return operation;
        }
    }
}
