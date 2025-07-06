using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.Buffers;
public readonly unsafe struct NativeArray<T> : IDisposable
{
    private readonly nint _ptr;
    private readonly int _length;

    public ref T this[int index]
    {
        get {
            Throws.ThrowIfIndexGreaterThanOrEqual(index, _length);
            return ref Unsafe.AsRef<T>((void*)(_ptr + index));
        }
    }

    public int Length => _length;

    public NativeArray(int length)
    {
        Throws.ThrowIfNegative(length); 
        _length = length;
        _ptr = Marshal.AllocHGlobal(length);
    }

    public Span<T> AsSpan() => new Span<T>((void*)_ptr, _length);

    public ref T GetPinnableReference()
    {
        if (_length == 0)
            return ref Unsafe.NullRef<T>();
        return ref Unsafe.AsRef<T>((void*)_ptr);
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal(_ptr);
    }
}
