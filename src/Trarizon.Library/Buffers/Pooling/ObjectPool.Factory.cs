namespace Trarizon.Library.Buffers.Pooling;
public enum ObjectPoolKind
{
    /// <summary>
    /// The simplest object pool, without tracking rented objects.
    /// </summary>
    Simple,
    /// <summary>
    /// The default object pool, with tracking rented objects.
    /// </summary>
    Tracked,
    /// <summary>
    /// The thread-safe object pool, with tracking rented objects.
    /// </summary>
    ThreadSafe,
}
partial class ObjectPool<T>
{
    public static ObjectPool<T> Create(Func<T> createFactory,
        Action<T>? onRented = null, Action<T>? onReturned = null, Action<T>? onReleased = null,
        ObjectPoolKind kind = ObjectPoolKind.Tracked, int maxCount = -1)
    {
        return kind switch
        {
            ObjectPoolKind.Tracked => new TrackedObjectPool<T>(createFactory, onRented, onReturned, onReleased, maxCount),
            ObjectPoolKind.Simple => new SimpleObjectPool<T>(createFactory, onRented, onReturned, onReleased, maxCount),
            ObjectPoolKind.ThreadSafe => new ThreadSafeObjectPool<T>(createFactory, onRented, onReturned, onReleased, maxCount),
            _ => TraThrow.InvalidEnumState<SimpleObjectPool<T>>(kind),
        };
    }
}
