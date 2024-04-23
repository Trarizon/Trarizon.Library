using System.Runtime.CompilerServices;

namespace Trarizon.Library.Helpers;
public static class UnsafeHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly TTo AsReadOnly<TFrom, TTo>(ref readonly TFrom source)
        => ref Unsafe.As<TFrom, TTo>(ref Unsafe.AsRef(in source));

    public static nint Offset<T>(ref readonly T origin, ref readonly T target)
    {
        return Unsafe.ByteOffset(in origin, in target) / Unsafe.SizeOf<T>();
    }
}
