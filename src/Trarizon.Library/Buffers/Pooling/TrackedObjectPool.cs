using Trarizon.Library.Collections;
#if NETSTANDARD
using ArrU = Trarizon.Library.Collections.TraCollection;
#else
using ArrU = System.Array;
#endif

namespace Trarizon.Library.Buffers.Pooling;
public sealed class TrackedObjectPool<T> : ObjectPool<T> where T : class
{
    private readonly Stack<T> _pooled;
    private readonly List<T> _rented;
    private readonly int _maxCount;
    private readonly Func<T> _createFactory;
    private readonly Action<T>? _onRent;
    private readonly Action<T>? _onReturn;
    private readonly Action<T>? _onDispose;

    public TrackedObjectPool(Func<T> createFactory, Action<T>? onRent = null, Action<T>? onReturn = null, Action<T>? onDispose = null, int maxCount = -1)
    {
        _createFactory = createFactory;
        _onRent = onRent;
        _onReturn = onReturn;
        _onDispose = onDispose;
        _pooled = new();
        _rented = [];
        _maxCount = maxCount < 0 ? ArrU.MaxLength : maxCount;
    }

    /// <inheritdoc />
    public override void ReleasePooled()
    {
        while (_pooled.TryPop(out var item)) {
            _onDispose?.Invoke(item);
        }
    }

    public override T Rent()
    {
        T item;
        if (_pooled.TryPop(out var resItem)) {
            item = resItem;
            _onRent?.Invoke(item);
            _rented.Add(item);
        }
        else {
            if (_rented.Count == _maxCount) {
                item = default!;
                Throws.ThrowInvalidOperation("Failed to rent new item, object pool is full");
            }
            item = _createFactory();
            _rented.Add(item);
        }
        return item;
    }

    public override void Return(T item)
    {
        var index = _rented.IndexOf(item);
        if (index < 0) {
            Throws.ThrowInvalidOperation("Try to return a object that is not pooled.");
        }

        _rented.RemoveAt(index);
        _pooled.Push(item);
        _onReturn?.Invoke(item);
    }
}
