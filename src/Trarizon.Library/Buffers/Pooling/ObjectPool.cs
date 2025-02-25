namespace Trarizon.Library.Buffers.Pooling;
public abstract partial class ObjectPool<T> : IObjectAllocator<T> where T : class
{
    /// <summary>
    /// Release all unrented objects.
    /// </summary>
    public abstract void ReleasePooled();

    public abstract T Rent();

    public abstract void Return(T item);

    T IObjectAllocator<T>.Allocate() => Rent();
    void IObjectAllocator<T>.Release(T item) => Return(item);
}
