namespace Trarizon.Library.Collections;
public static partial class TraSpan
{
    public static int FindIndex<T, TArgs>(this ReadOnlySpan<T> span, TArgs args, Func<T, TArgs, bool> predicate)
    {
        for (int i = 0; i < span.Length; i++) {
            if (predicate(span[i], args))
                return i;
        }
        return -1;
    }

    public static int FindIndex<T, TArgs>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        for (int i = 0; i < span.Length; i++) {
            if (predicate(span[i]))
                return i;
        }
        return -1;
    }

    public static T? Find<T, TArgs>(this ReadOnlySpan<T> span, TArgs args, Func<T, TArgs, bool> predicate)
    {
        foreach (T item in span) {
            if (predicate(item, args))
                return item;
        }
        return default;
    }

    public static T? Find<T, TArgs>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        foreach (T item in span) {
            if (predicate(item))
                return item;
        }
        return default;
    }
}