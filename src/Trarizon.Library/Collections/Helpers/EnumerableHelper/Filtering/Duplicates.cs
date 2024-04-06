using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    [Experimental(ExperimentalDiagnosticIds.EnumerableQuery_Duplicates)]
    public static IEnumerable<T> Duplicates<T>(this IEnumerable<T> source)
        => source.GroupBy(x => x)
        .Where(g => g.Count() > 1)
        .SelectMany(g => g);

    [Experimental(ExperimentalDiagnosticIds.EnumerableQuery_Duplicates)]
    public static IEnumerable<T> DuplicatesBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        => source.GroupBy(keySelector)
        .Where(g => g.Count() > 1)
        .SelectMany(g => g);

}
