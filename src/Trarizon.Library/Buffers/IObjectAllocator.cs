namespace Trarizon.Library.Buffers;
public interface IObjectAllocator<T> where T : class
{
    T Allocate();
    void Release(T item);

    public struct AutoReleaseScope<TAllocator> : IDisposable where TAllocator : IObjectAllocator<T>
    {
        private readonly TAllocator _allocator;
        public T Item { get; private set; }

        internal AutoReleaseScope(TAllocator allocator, T item)
        {
            _allocator = allocator;
            Item = item;
        }

        public void Dispose()
        {
            _allocator.Release(Item);
            Item = null!;
        }
    }
}

public static class IObjectAllocatorExt
{
    public static IObjectAllocator<T>.AutoReleaseScope<IObjectAllocator<T>> Allocate<T>(this IObjectAllocator<T> allocator, out T item) where T : class
    {
        item = allocator.Allocate();
        return new IObjectAllocator<T>.AutoReleaseScope<IObjectAllocator<T>>(allocator, item);
    }
}
