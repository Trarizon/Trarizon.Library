using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    [Experimental(ExperimentalDiagnosticIds.EnumerableHelper_Continue)]
    public static IEnumerable<T> Continue<T>(IEnumerator<T> enumerator)
    {
        try {
            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
            }
        } finally {
            enumerator.Dispose();
        }
    }
}
