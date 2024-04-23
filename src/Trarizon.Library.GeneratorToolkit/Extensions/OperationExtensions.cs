using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class OperationExtensions
{
    public static IEnumerable<IOperation> Ancestors(this IOperation operation, bool includeSelf = false)
    {
        return (includeSelf ? operation : operation.Parent)
            .EnumerateByWhileNotNull(o => o.Parent);
    }
}
