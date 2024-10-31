using System.Buffers;

namespace Trarizon.Library.Buffers;
public static class TraArrayPool
{
    public static PooledArrayAutoReturnScope<T> Rent<T>(this ArrayPool<T> pool, int minLength, out T[] array)
    {
        array = pool.Rent(minLength);
        return new PooledArrayAutoReturnScope<T>(pool, array);
    }

    public readonly struct PooledArrayAutoReturnScope<T> : IDisposable
    {
        private readonly ArrayPool<T> _pool;
        private readonly T[] _array;

        internal PooledArrayAutoReturnScope(ArrayPool<T> pool, T[] array)
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
