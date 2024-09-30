using Microsoft.CodeAnalysis;
using Trarizon.Library.GeneratorToolkit.CoreLib.Collections;

namespace Trarizon.Library.GeneratorToolkit.ContextModelExtensions;
public static class OperationExtensions
{
    public static IEnumerable<IOperation> Ancestors(this IOperation operation, bool includeSelf = false)
        => (includeSelf ? operation : operation.Parent)
            .EnumerateByWhileNotNull(o => o.Parent);
}
