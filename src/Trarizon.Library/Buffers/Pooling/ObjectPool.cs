using CommunityToolkit.Diagnostics;

namespace Trarizon.Library.Buffers.Pooling;
public enum ObjectPoolKind
{
    /// <summary>
    /// The default object pool, with tracking rented objects.
    /// </summary>
    Default,
    /// <summary>
    /// The simplest object pool, without tracking rented objects.
    /// </summary>
    Simple,
    /// <summary>
    /// The thread-safe object pool, with tracking rented objects.
    /// </summary>
    ThreadSafe,
}
public abstract class ObjectPool<T> : IObjectAllocator<T> where T : class
{
    public static ObjectPool<T> Create(Func<T> createFactory,
        Action<T>? onRent = null, Action<T>? onReturn = null, Action<T>? onDispose = null,
        ObjectPoolKind kind = ObjectPoolKind.Default, int maxCount = -1)
    {
        return kind switch
        {
            ObjectPoolKind.Default => new DefaultObjectPool<T>(createFactory, onRent, onReturn, onDispose, maxCount),
            ObjectPoolKind.Simple => new SimpleObjectPool<T>(createFactory, onRent, onReturn, onDispose, maxCount),
            ObjectPoolKind.ThreadSafe => new ThreadSafeObjectPool<T>(createFactory, onRent, onReturn, onDispose, maxCount),
            _ => TraThrow.InvalidEnumState<ObjectPool<T>>(kind),
        };
    }

    /// <summary>
    /// Dispose all unrented objects.
    /// </summary>
    public abstract void ReleasePooled();

    public abstract AutoReturnScope Rent(out T item);

    public abstract void Return(T item);

    public virtual T Rent()
    {
        Rent(out var item);
        return item;
    }

    T IObjectAllocator<T>.Allocate() => Rent();
    void IObjectAllocator<T>.Release(T item) => Return(item);

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
