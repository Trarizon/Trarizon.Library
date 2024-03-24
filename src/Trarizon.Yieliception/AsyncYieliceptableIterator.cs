using System.Diagnostics;
using Trarizon.Library.Collections.Helpers;
using Trarizon.Library.Helpers;
using Trarizon.Yieliception.Components;
using Trarizon.Yieliception.Yieliceptors;

namespace Trarizon.Yieliception;
public class AsyncYieliceptableIterator<T>(
    IAsyncEnumerator<IYieliceptor<T>?> enumerator,
    IAsyncYieliceptionComponent[] components) : IYieliceptableIterator<T>, IAsyncDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private IAsyncEnumerator<IYieliceptor<T>?>? _enumerator = enumerator;

    public event Action<AsyncYieliceptableIterator<T>>? End;

    public bool IsEnded => _enumerator is null;
    public IYieliceptor<T>? CurrentYieliceptor => _enumerator?.Current;
    public IAsyncYieliceptionComponent[] Components => components;

    public ValueTask<YieliceptionResult> NextAsync(T args)
        => IsEnded ? ValueTask.FromResult(YieliceptionResult.Ended) : ThreadSafeNextAsync(true, args);

    public ValueTask<YieliceptionResult> ForceNextAsync(bool returnIfOccupied)
    {
        if (IsEnded)
            return ValueTask.FromResult(YieliceptionResult.Ended);
        else if (returnIfOccupied && _semaphore.CurrentCount == 0)
            return ValueTask.FromResult(YieliceptionResult.Occupied);
        else
            return ThreadSafeNextAsync(false, default!);
    }

    private async ValueTask<YieliceptionResult> ThreadSafeNextAsync(bool check, T args)
    {
        Debug.Assert(_enumerator is not null);

        _semaphore.Wait();
        try {
            if (check && _enumerator.Current?.CanMoveNext(args) == false) {
                foreach (var c in components)
                    c.OnNext(this, false);
                return YieliceptionResult.Rejected;
            }
            else if (await _enumerator!.MoveNextAsync()) {
                foreach (var c in components)
                    c.OnNext(this, true);
                return YieliceptionResult.Moved;
            }
            else {
                End?.Invoke(this);
                await DisposeAsync();
                return YieliceptionResult.Ended;
            }
        } finally {
            _semaphore.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var component in components) {
            component.Dispose();
        }
        await _enumerator?.DisposeAsync();
        _enumerator = null;
        _semaphore.Dispose();
        GC.SuppressFinalize(this);
    }

    YieliceptionResult IYieliceptableIterator<T>.Next(T args) => NextAsync(args).Sync();
    YieliceptionResult IYieliceptableIterator<T>.ForceNext(bool returnIfOccupied) => ForceNextAsync(returnIfOccupied).Sync();
}
