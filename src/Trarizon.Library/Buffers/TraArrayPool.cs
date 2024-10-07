using System.Buffers;

namespace Trarizon.Library.Buffers;
public static class TraArrayPool
{
    public static PooledArray<T> Rent<T>(this ArrayPool<T> pool, int minLength, out T[] array)
    {
        array = pool.Rent(minLength);
        return new PooledArray<T>(pool, array);
    }

    public readonly struct PooledArray<T> : IDisposable
    {
        private readonly ArrayPool<T> _pool;
        private readonly T[] _array;

        internal PooledArray(ArrayPool<T> pool, T[] array)
        {
            _pool = pool;
            _array = array;
        }

        void IDisposable.Dispose()
        {
            _pool?.Return(_array);
        }
    }
}
