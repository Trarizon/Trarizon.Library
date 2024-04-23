using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    [Experimental(ExperimentalDiagnosticIds.EnumerableQuery_Duplicates)]
    public static IEnumerable<T> Range<T>(int start, int count, Func<int, T> selector)
    {
        int end = start + count;
        for (int i = start; i < end; i++) {
            yield return selector(i);
        }
    }
}
