global using AsyncYieliceptableIterator = Trarizon.Yieliception.AsyncYieliceptableIterator<object>;
global using YieliceptableIterator = Trarizon.Yieliception.YieliceptableIterator<object>;
global using IYieliceptor = Trarizon.Yieliception.Yieliceptors.IYieliceptor<object>;
using Trarizon.Yieliception.Components;
using Trarizon.Yieliception.Yieliceptors;

namespace Trarizon.Yieliception;
public static class YieliceptionExtensions
{
    public static YieliceptableIterator<T> ToYieliceptable<T>(this IEnumerator<IYieliceptor<T>?> enumerator,
        bool threadSafe = true, params IYieliceptionComponent[] components)
        => new(enumerator, threadSafe, components);

    public static AsyncYieliceptableIterator<T> ToYieliceptable<T>(this IAsyncEnumerator<IYieliceptor<T>?> enumerator,
        params IAsyncYieliceptionComponent[] components)
        => new(enumerator, components);

    public static YieliceptionResult Next(this YieliceptableIterator iterator)
        => iterator.Next(default!);

    public static ValueTask<YieliceptionResult> NextAsync(this AsyncYieliceptableIterator iterator)
        => iterator.NextAsync(default!);

    #region Specialized

    /// <summary>
    /// Provide communication between iterator and caller
    /// </summary>
    public static bool SendAndNext<TYield, TArgs>(this IEnumerator<ValueDeliverer<TYield, TArgs>?> enumerator,
        TArgs args, out TYield? yieldValue)
    {
        // This statement has 2 meadning:
        // 1. Enumerator hasn't started, and this invocation is the initialization
        // 2. Enumerator yields null, thus we can ignore the value assignment
        if (enumerator.Current != null) {
            // This behavior equals to CanMoveNext()
            enumerator.Current.ReceivedValue = args;
        }

        if (enumerator.MoveNext()) {
            yieldValue = enumerator.Current is { } current ? current.YieldValue : default;
            return true;
        }
        else {
            yieldValue = default;
            return false;
        }
    }

    #endregion
}
