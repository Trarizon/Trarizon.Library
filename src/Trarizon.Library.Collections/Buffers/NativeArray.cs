using CommunityToolkit.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections.Buffers;
public readonly unsafe struct NativeArray<T> : IDisposable
{
    private readonly nint _ptr;
    private readonly int _length;

    public ref T this[int index]
    {
        get {
            Guard.IsLessThan((nuint)index, (nuint)_length);
            return ref Unsafe.AsRef<T>((void*)(_ptr + index));
        }
    }

    public int Length => _length;

    public NativeArray(int length)
    {
        Guard.IsGreaterThanOrEqualTo(length, 0);
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
