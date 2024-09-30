namespace Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
partial class TraEnumerable
{
    /// <summary>
    /// yield <paramref name="first"/>, and then
    /// repeatly call <paramref name="nextSelector"/> to create next value.
    /// Enumerate stop until <paramref name="predicate"/> returns false.
    /// </summary>
    public static IEnumerable<T> EnumerateByWhile<T>(this T first, Func<T, T> nextSelector, Func<T, bool> predicate)
    {
        var val = first;
        while (predicate(val)) {
            yield return val;
            val = nextSelector(val);
        }
    }

    /// <summary>
    /// yield <paramref name="first"/>, and then
    /// repeatly call <paramref name="nextWhereSelector"/> to create next value.
    /// Enumerate stop until current value is null.
    /// </summary>
    public static IEnumerable<T> EnumerateByWhileNotNull<T>(this T? first, Func<T, T?> nextSelector) where T : class
    {
        var val = first;
        while (val is not null) {
            yield return val;
            val = nextSelector(val);
        }
    }

    /// <summary>
    /// yield <paramref name="first"/>, and then
    /// repeatly call <paramref name="nextSelector"/> to create next value.
    /// Enumerate stop until current value is null.
    /// </summary>
    public static IEnumerable<T> EnumerateByWhileNotNull<T>(this T? first, Func<T, T?> nextSelector) where T : struct
    {
        var val = first;
        while (val is not null) {
            yield return val.GetValueOrDefault();
            val = nextSelector(val.GetValueOrDefault());
        }
    }
}
