namespace Trarizon.Library.Buffers.Pooling;
public abstract class ObjectPool<T> where T : class
{
    public static ObjectPool<T> Create(Func<T> createFactory,
        Action<T>? onRent = null, Action<T>? onReturn = null, Action<T>? onDispose = null,
        bool threadSafe = false, int maxCount = -1)
    {
        if (threadSafe)
            return new ThreadSafeObjectPool<T>(createFactory, onRent, onReturn, onDispose, maxCount);
        else
            return new DefaultObjectPool<T>(createFactory, onRent, onReturn, onDispose, maxCount);
    }

    /// <summary>
    /// Dispose all unrented objects.
    /// </summary>
    public abstract void TrimExcess();

    public abstract AutoReturnScope Rent(out T item);

    public abstract void Return(T item);

    public virtual T Rent()
    {
        Rent(out var item);
        return item;
    }

    public readonly struct AutoReturnScope : IDisposable
    {
        private readonly ObjectPool<T> _pool;
        public readonly T Item;

        internal AutoReturnScope(ObjectPool<T> pool, T item)
        {
            _pool = pool;
            Item = item;
        }

        public void Dispose() => _pool?.Return(Item);
    }
}
