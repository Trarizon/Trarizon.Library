using Trarizon.Library.Collections;
#if NETSTANDARD
using ArrayMaxLengthProvider = Trarizon.Library.Collections.TraCollection;
#else
using ArrayMaxLengthProvider = System.Array;
#endif

namespace Trarizon.Library.Buffers.Pooling;
public sealed partial class SimpleObjectPool<T> : ObjectPool<T> where T : class
{
    private readonly Stack<T> _pooled;
    private readonly int _maxCount;
    private readonly Func<T> _createFactory;
    private readonly Action<T>? _onRent;
    private readonly Action<T>? _onReturn;
    private readonly Action<T>? _onDispose;

    public SimpleObjectPool(Func<T> createFactory, Action<T>? onRent = null, Action<T>? onReturn = null, Action<T>? onDispose = null, int maxCount = -1)
    {
        _createFactory = createFactory;
        _onRent = onRent;
        _onReturn = onReturn;
        _onDispose = onDispose;
        _pooled = new();
        _maxCount = maxCount < 0 ? ArrayMaxLengthProvider.MaxLength : maxCount;
    }

    /// <inheritdoc />
    public override void ReleasePooled()
    {
        if (_onDispose is null) {
            _pooled.Clear();
            return;
        }

        while (_pooled.TryPop(out var item)) {
            _onDispose.Invoke(item);
        }
    }

    public override T Rent()
    {
        T item;
        if (_pooled.TryPop(out var resItem)) {
            item = resItem;
            _onRent?.Invoke(item);
            return item;
        }
        else {
            item = _createFactory();
        }
        return item;
    }

    public override void Return(T item)
    {
        if (_pooled.Count == _maxCount) {
            _onReturn?.Invoke(item);
            _onDispose?.Invoke(item);
            return;
        }
        _pooled.Push(item);
        _onReturn?.Invoke(item);
    }
}
