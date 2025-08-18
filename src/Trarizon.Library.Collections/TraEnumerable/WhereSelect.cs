namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    public delegate bool TrySelector<T, TResult>(T value, out TResult result);

    public static IEnumerable<TResult> WhereSelect<T, TResult>(this IEnumerable<T> source, TrySelector<T, TResult> selector)
    {
        TResult result = default!;
        return source.Where(x => selector(x, out result)).Select(_ => result!);
    }
}
