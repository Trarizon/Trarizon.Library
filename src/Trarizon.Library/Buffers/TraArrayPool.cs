using System.Buffers;

namespace Trarizon.Library.Buffers;
public static class TraArrayPool
{
    public static PooledArrayAutoReturnScope<T> Rent<T>(this ArrayPool<T> pool, int minLength, out T[] array, bool clearArrayWhenReturn = false)
    {
        array = pool.Rent(minLength);
        return new PooledArrayAutoReturnScope<T>(pool, array, clearArrayWhenReturn);
    }

    public readonly struct PooledArrayAutoReturnScope<T> : IDisposable
    {
        private readonly ArrayPool<T> _pool;
        private readonly T[] _array;
        private readonly bool _clearArray;

        internal PooledArrayAutoReturnScope(ArrayPool<T> pool, T[] array, bool clearArray)
        {
            _pool = pool;
            _array = array;
            _clearArray = clearArray;
        }

        void IDisposable.Dispose()
        {
            _pool?.Return(_array, _clearArray);
        }
    }
}
