using CommunityToolkit.HighPerformance;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Wrappers;

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    /// <remarks>
    /// Official LinQ has some internal optimizations for linq chain on <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})"/>,
    /// so i suggest not using this if <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})"/> meets your requirements.
    /// </remarks>
    public static bool TryFirst<T>(this IEnumerable<T> source, [MaybeNullWhen(false)] out T value)
    {
        if (source.TryGetNonEnumeratedCount(out var count)) {
            if (count > 0) {
                if (source is IList<T> list) {
                    value = list[0];
                    return true;
                }
                goto ByEnumerate;
            }
            else {
                value = default;
                return false;
            }
        }

    ByEnumerate:

        using var enumerator = source.GetEnumerator();

        if (enumerator.MoveNext()) {
            value = enumerator.Current;
            return true;
        }
        else {
            value = default;
            return false;
        }
    }

    public static bool TryFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, [MaybeNullWhen(false)] out T value)
    {
        using var enumerator = source.GetEnumerator();

        while (enumerator.MoveNext()) {
            var current = enumerator.Current;
            if (predicate(current)) {
                value = current;
                return true;
            }
        }
        value = default;
        return false;
    }

    public static bool TryLast<T>(this IEnumerable<T> source, [MaybeNullWhen(false)] out T value)
    {
        if (source.TryGetNonEnumeratedCount(out var count)) {
            if (count > 0) {
                if (source is IList<T> list) {
                    value = list[^1];
                    return true;
                }
                goto ByEnumerate;
            }
            else {
                value = default;
                return false;
            }
        }

    ByEnumerate:

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) {
            value = default;
            return false;
        }

        value = enumerator.Current;

        while (enumerator.MoveNext()) {
            value = enumerator.Current;
        }
        return true;
    }

    public static bool TryLast<T>(this IEnumerable<T> source, Func<T, bool> predicate, [MaybeNullWhen(false)] out T value)
    {
        using var enumerator = source.GetEnumerator();
        Optional<T> val = default;
        while (enumerator.MoveNext()) {
            var current = enumerator.Current;
            if (predicate(current)) {
                val = current;
            }
        }

        return val.TryGetValue(out value);
    }

    // 改成BoundedMax？，这样的话换个文件。。
    // 话说这是Aggregation还是Element？ 问了GPT感觉是Aggregation，因为需要考虑整个集合，不像First找到要返回的就完了
    // 坏了，但是，GPT说这个方法应该是Element
    // 
    // 因为找不到我在哪里用到了这个东西，所以我不知道FirstBy的要不要返回Key了
    // 
    // 我需不需要标记一下有没有>=，虽然Single貌似不标记这个
    public static T FirstNearToMax<T>(this IEnumerable<T> source, T max)
        => source.FirstNearToMax(max, Comparer<T>.Default);

    public static T FirstNearToMax<T, TComparer>(this IEnumerable<T> source, T max, TComparer comparer) where TComparer : IComparer<T>
    {
        var res = TryGetFirstNearToMax(source, max, comparer, out var exists);
        if (!exists)
            TraThrow.NoElement();
        return res;
    }

    public static T? FirstNearToMaxOrDefault<T>(this IEnumerable<T> source, T max)
        => source.FirstNearToMaxOrDefault(max, Comparer<T>.Default);

    public static T? FirstNearToMaxOrDefault<T, TComparer>(this IEnumerable<T> source, T max, TComparer comparer) where TComparer : IComparer<T>
        => TryGetFirstNearToMax(source, max, comparer, out _);

    private static T TryGetFirstNearToMax<T, TComparer>(IEnumerable<T> source, T max, TComparer comparer, out bool exists) where TComparer : IComparer<T>
    {
        if (source.TryGetSpan(out var span)) {
            if (span.IsEmpty) {
                exists = false;
                return default!;
            }
            exists = true;
            var value = span.DangerousGetReferenceAt(0);
            if (comparer.Compare(value, max) >= 0)
                return value;

            foreach (var item in span[1..]) {
                if (comparer.Compare(item, max) >= 0)
                    return item;
                if (comparer.Compare(item, value) > 0)
                    value = item;
            }
            return value;
        }
        else {
            using var enumerator = source.GetEnumerator();


            if (!enumerator.MoveNext()) {
                exists = false;
                return default!;
            }
            exists = true;

            var value = enumerator.Current;
            if (comparer.Compare(value, max) >= 0)
                return value;

            while (enumerator.MoveNext()) {
                var current = enumerator.Current;

                if (comparer.Compare(current, max) >= 0)
                    return current;
                if (comparer.Compare(current, value) > 0)
                    value = current;
            }
            return value;
        }
    }

    public static (TKey Key, T Value) FirstNearToMaxBy<T, TKey>(this IEnumerable<T> source, TKey max, Func<T, TKey> keySelector)
        => source.FirstNearToMaxBy(max, keySelector);

    public static (TKey Key, T Value) FirstNearToMaxBy<T, TKey, TComparer>(IEnumerable<T> source, TKey max, Func<T, TKey> keySelector, TComparer comparer) where TComparer : IComparer<TKey>
    {
        var res = TryGetFirstNearToMaxBy(source, max, keySelector, comparer, out var exists);
        if (!exists)
            TraThrow.NoElement();
        return res;
    }

    /// <summary>
    /// Find the first item has priority &gt;= <paramref name="maxProirity"/>,
    /// if not found, return the first item with greatest priority
    /// </summary>
    public static (TKey Key, T Value) FirstNearToMaxByOrDefault<T, TKey>(this IEnumerable<T> source, TKey maxProirity, Func<T, TKey> keySelector)
        => source.FirstNearToMaxByOrDefault(maxProirity, keySelector, Comparer<TKey>.Default);

    public static (TKey Key, T Value) FirstNearToMaxByOrDefault<T, TKey, TComparer>(this IEnumerable<T> source, TKey maxPriority, Func<T, TKey> keySelector, TComparer comparer) where TComparer : IComparer<TKey>
        => TryGetFirstNearToMaxBy(source, maxPriority, keySelector, comparer, out _);

    private static (TKey, T) TryGetFirstNearToMaxBy<T, TKey, TComparer>(IEnumerable<T> source, TKey max, Func<T, TKey> keySelector, TComparer comparer, out bool exists) where TComparer : IComparer<TKey>
    {
        if (source.TryGetSpan(out var span)) {
            if (span.IsEmpty) {
                exists = false;
                return default;
            }
            exists = true;
            var value = span.DangerousGetReferenceAt(0);
            var key = keySelector(value);
            if (comparer.Compare(key, max) >= 0)
                return (key, value);

            foreach (var item in span[1..]) {
                var curKey = keySelector(item);
                if (comparer.Compare(curKey, max) >= 0)
                    return (curKey, item);
                if (comparer.Compare(curKey, key) > 0)
                    (key, value) = (curKey, item);
            }
            return (key, value);
        }
        else {
            using var enumerator = source.GetEnumerator();


            if (!enumerator.MoveNext()) {
                exists = false;
                return default;
            }
            exists = true;

            var value = enumerator.Current;
            var key = keySelector(value);
            if (comparer.Compare(key, max) >= 0)
                return (key, value);

            while (enumerator.MoveNext()) {
                var current = enumerator.Current;
                var curKey = keySelector(current);

                if (comparer.Compare(curKey, max) >= 0)
                    return (curKey, current);
                if (comparer.Compare(curKey, key) > 0)
                    (key, value) = (curKey, current);
            }
            return (key, value);
        }
    }
}
