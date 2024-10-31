using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Trarizon.Library.Buffers.Pooling;
internal sealed class ThreadSafeObjectPool<T> : ObjectPool<T> where T : class
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

    public override void TrimExcess()
    {
        while (_pooled.TryTake(out var item))
        {
            _onDispose?.Invoke(item);
        }
    }

    public override AutoReturnScope Rent(out T item)
    {
        if (_pooled.TryTake(out var resItem))
        {
            item = resItem;
            _onRent?.Invoke(item);
            var added = _rented.TryAdd(item, default);
            Debug.Assert(added);
            return new AutoReturnScope(this, item);
        }
        else
        {
            while (true)
            {
                var curCount = _count;
                if (_count == _maxCount)
                {
                    // Final check if there's item returned
                    if (_pooled.TryTake(out resItem))
                    {
                        item = resItem;
                        _onRent?.Invoke(item);
                        var added = _rented.TryAdd(item, default);
                        Debug.Assert(added);
                        return new AutoReturnScope(this, item);
                    }
                    else
                    {
                        item = default!;
                        return ThrowHelper.ThrowInvalidOperationException<AutoReturnScope>("Failed to rent new item, object pool is full");
                    }
                }

                // _count not modified, create new item
                if (Interlocked.CompareExchange(ref _count, curCount + 1, curCount) == curCount)
                {
                    item = _createFactory();
                    var added = _rented.TryAdd(item, default);
                    Debug.Assert(added);
                    return new AutoReturnScope(this, item);
                }
                // _count is modified, continue loop to re-check
            }
        }
    }

    public override void Return(T item)
    {
        if (_rented.TryRemove(item, out _))
        {
            _onReturn?.Invoke(item);
            _pooled.Add(item);
        }
    }
}
