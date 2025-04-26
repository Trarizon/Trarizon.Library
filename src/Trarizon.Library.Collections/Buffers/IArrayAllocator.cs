using System.Buffers;

namespace Trarizon.Library.Collections.Buffers;
public interface IArrayAllocator<T>
{
    T[] Allocate(int minLength);
    void Release(T[] array, bool clearArray);

    public readonly struct AutoReleaseScope<TAllocator> : IDisposable where TAllocator : IArrayAllocator<T>
    {
        private readonly TAllocator _allocator;
        private readonly T[]? _array;
        private readonly bool _clearArray;

        public T[] Array => _array!;

        internal AutoReleaseScope(TAllocator allocator, T[] array, bool clearArray)
        {
            _allocator = allocator;
            _array = array;
            _clearArray = clearArray;
        }

        public void Dispose()
        {
            _allocator?.Release(Array, _clearArray);
        }
    }
}

public static class IArrayAllocatorExt
{
    public static IArrayAllocator<T>.AutoReleaseScope<IArrayAllocator<T>> Allocate< T>(this IArrayAllocator<T> allocator, int minLength, bool clearArray, out T[] array)
    {
        array = allocator.Allocate(minLength);
        return new IArrayAllocator<T>.AutoReleaseScope<IArrayAllocator<T>>(allocator, array, clearArray);
    }

    public static IArrayAllocator<T>.AutoReleaseScope<ArrayPoolAllocator<T>> Rent<T>(this ArrayPool<T> pool, int minLength, out T[] array, bool clearArrayRelease = false)
    {
        array = pool.Rent(minLength);
        return new IArrayAllocator<T>.AutoReleaseScope<ArrayPoolAllocator<T>>(new ArrayPoolAllocator<T>(pool), array, clearArrayRelease);
    }

    public readonly struct ArrayPoolAllocator<T> : IArrayAllocator<T>
    {
        private readonly ArrayPool<T> _pool;

        internal ArrayPoolAllocator(ArrayPool<T> pool)
        {
            _pool = pool;
        }

        public T[] Allocate(int minLength) => _pool.Rent(minLength);
        public void Release(T[] array, bool clearArray) => _pool.Return(array, clearArray);
    }
}
