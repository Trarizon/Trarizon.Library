using System;
using System.Collections.Generic;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class EnumerableExtensions
{
    #region TryFirst

    public static bool TryFirst<T>(this IEnumerable<T> source, out T value, T defaultValue = default!)
    {
        if (source is IList<T> list) {
            if (list.Count > 0) {
                value = list[0];
                return true;
            }
            else {
                value = defaultValue;
                return false;
            }
        }

        using var enumerator = source.GetEnumerator();

        if (enumerator.MoveNext()) {
            value = enumerator.Current;
            return true;
        }
        else {
            value = defaultValue;
            return false;
        }
    }

    public static bool TryFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T value, T defaultValue = default!)
    {
        using var enumerator = source.GetEnumerator();

        while (enumerator.MoveNext()) {
            var current = enumerator.Current;
            if (predicate(current)) {
                value = current;
                return true;
            }
        }
        value = defaultValue;
        return false;
    }

    #endregion
}
