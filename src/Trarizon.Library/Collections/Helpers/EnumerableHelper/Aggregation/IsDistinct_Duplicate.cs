using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections.AllocOpt;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    public static bool IsDistinct<T>(this IEnumerable<T> source, IEqualityComparer<T>? comparer = null)
    {
        AllocOptSet<T> set = new(comparer);
        foreach (var item in source) {
            if (!set.Add(item))
                return false;
        }
        return false;
    }

    public static bool IsDistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        AllocOptSet<TKey> set = [];
        foreach (var item in source) {
            if (!set.Add(keySelector(item)))
                return false;
        }
        return true;
    }


    [Experimental(ExperimentalDiagnosticIds.EnumerableQuery_Duplicates)]
    public static IEnumerable<T> Duplicates<T>(this IEnumerable<T> source)
        => source.GroupBy(x => x)
        .Where(g => g.Count() > 1)
        .SelectMany(g => g);

    [Experimental(ExperimentalDiagnosticIds.EnumerableQuery_Duplicates)]
    public static IEnumerable<T> DuplicatesBy<T,TKey>(this IEnumerable<T> source,Func<T,TKey> keySelector)
        => source.GroupBy(keySelector)
        .Where(g => g.Count() > 1)
        .SelectMany(g => g);
}
