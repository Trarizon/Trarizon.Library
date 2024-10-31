using CommunityToolkit.Diagnostics;

namespace Trarizon.Library.Buffers.Pooling;
internal sealed class DefaultObjectPool<T> : ObjectPool<T> where T : class
{
    private readonly Stack<T> _pooled;
    private readonly List<T> _rented;
    private readonly int _maxCount;
    private readonly Func<T> _createFactory;
    private readonly Action<T>? _onRent;
    private readonly Action<T>? _onReturn;
    private readonly Action<T>? _onDispose;

    public DefaultObjectPool(Func<T> createFactory, Action<T>? onRent = null, Action<T>? onReturn = null, Action<T>? onDispose = null, int maxCount = -1)
    {
        _createFactory = createFactory;
        _onRent = onRent;
        _onReturn = onReturn;
        _pooled = new();
        _rented = [];
        _maxCount = maxCount < 0 ? Array.MaxLength : maxCount;
    }

    /// <summary>
    /// Dispose all unrented objects.
    /// </summary>
    public override void TrimExcess()
    {
        while (_pooled.TryPop(out var item))
        {
            _onDispose?.Invoke(item);
        }
    }

    public override AutoReturnScope Rent(out T item)
    {
        if (_pooled.TryPop(out var resItem))
        {
            item = resItem;
            _onRent?.Invoke(item);
            _rented.Add(item);
            return new AutoReturnScope(this, item);
        }
        else
        {
            if (_rented.Count == _maxCount)
            {
                item = default!;
                return ThrowHelper.ThrowInvalidOperationException<AutoReturnScope>("Failed to rent new item, object pool is full");
            }
            item = _createFactory();
            _rented.Add(item);
            return new AutoReturnScope(this, item);
        }
    }

    public override void Return(T item)
    {
        var index = _rented.IndexOf(item);
        if (index < 0)
        {
            ThrowHelper.ThrowInvalidOperationException("Try to return a object that is not pooled.");
        }

        _rented.RemoveAt(index);
        _pooled.Push(item);
        _onReturn?.Invoke(item);
    }
}
