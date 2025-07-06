using System.Collections.Concurrent;
using System.Diagnostics;

namespace Trarizon.Library.Buffers.Pooling;
public sealed class ThreadSafeObjectPool<T> : ObjectPool<T> where T : class
{
    private readonly ConcurrentBag<T> _pooled;
    private readonly ConcurrentDictionary<T, ValueTuple> _rented;
    private int _count;
    private readonly int _maxCount;
    private readonly Func<T> _createFactory;
    private readonly Action<T>? _onRent;
    private readonly Action<T>? _onReturn;
    private readonly Action<T>? _onDispose;

    public ThreadSafeObjectPool(Func<T> createFactory, Action<T>? onRent = null, Action<T>? onReturn = null, Action<T>? onDispose = null, int maxCount = -1)
    {
        _pooled = [];
        _rented = [];
        _count = 0;

        _createFactory = createFactory;
        _onRent = onRent;
        _onReturn = onReturn;
        _onDispose = onDispose;
        _maxCount = maxCount;
    }

    public override void ReleasePooled()
    {
        while (_pooled.TryTake(out var item)) {
            _onDispose?.Invoke(item);
        }
    }

    public override T Rent()
    {
        if (_pooled.TryTake(out var resItem)) {
            var item = resItem;
            _onRent?.Invoke(item);
            var added = _rented.TryAdd(item, default);
            Debug.Assert(added);
            return item;
        }
        else {
            while (true) {
                var curCount = _count;
                if (_count == _maxCount) {
                    // Final check if there's item returned
                    if (_pooled.TryTake(out resItem)) {
                        var item = resItem;
                        _onRent?.Invoke(item);
                        var added = _rented.TryAdd(item, default);
                        Debug.Assert(added);
                        return item;
                    }
                    else {
                        Throws.ThrowInvalidOperation("Failed to rent new item, object pool is full");
                        return default!;
                    }
                }

                // _count not modified, create new item
                if (Interlocked.CompareExchange(ref _count, curCount + 1, curCount) == curCount) {
                    var item = _createFactory();
                    var added = _rented.TryAdd(item, default);
                    Debug.Assert(added);
                    return item;
                }
                // _count is modified, continue loop to re-check
            }
        }
    }

    public override void Return(T item)
    {
        if (_rented.TryRemove(item, out _)) {
            _onReturn?.Invoke(item);
            _pooled.Add(item);
        }
    }
}
