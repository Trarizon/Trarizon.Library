using System.Numerics;

namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    /// <summary>
    /// Aggregate, and returns all values in process
    /// </summary>
    public static IEnumerable<TResult> AggregateSelect<T, TAccumulate, TResult>(this IEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
    {
        if (source.IsEmptyArray())
            return [];

        return Iterate();
        IEnumerable<TResult> Iterate()
        {
            var accumulate = seed;
            foreach (var item in source) {
                accumulate = func(accumulate, item);
                yield return resultSelector(accumulate);
            }
        }
    }

    /// <summary>
    /// Aggregate, and returns all values in process
    /// </summary>
    public static IEnumerable<TAccumulate> AggregateSelect<T, TAccumulate>(this IEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
        => source.AggregateSelect(seed, func, x => x);

    /// <summary>
    /// Aggregate, and returns all values in process
    /// </summary>
    public static IEnumerable<T> AggregateSelect<T>(this IEnumerable<T> source, Func<T, T, T> func) where T : INumberBase<T>
        => source.AggregateSelect(T.Zero, func);
}
