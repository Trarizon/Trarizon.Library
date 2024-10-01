﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
public static partial class TraEnumerable
{
    private static bool TryIterate<T>(this IEnumerator<T> enumerator, int count, out int iteratedCount)
    {
        iteratedCount = 0;
        if (count <= 0)
            return true;

        while (enumerator.MoveNext()) {
            if (++iteratedCount >= count)
                return true;
        }
        return false;
    }

    private static bool TryMoveNext<T>(this IEnumerator<T> enumerator, [MaybeNullWhen(false)] out T current)
    {
        if (enumerator.MoveNext()) {
            current = enumerator.Current;
            return true;
        }
        else {
            current = default;
            return false;
        }
    }

    private static IEnumerable<T> Continue<T>(this IEnumerator<T> enumerator)
    {
        while (enumerator.MoveNext()) {
            yield return enumerator.Current;
        }
    }

    private static bool IsEmptyArray<T>(this IEnumerable<T> source) => source is T[] { Length: 0 };
}

partial class TraEnumerable
{
    public static bool TryGetNonEnumeratedCount<T>(this IEnumerable<T> source, out int count)
    {
        if (source is ICollection<T> genericCollection) {
            count = genericCollection.Count;
            return true;
        }

        if (source is ICollection collection) {
            count = collection.Count;
            return true;
        }

        count = default;
        return false;
    }
}