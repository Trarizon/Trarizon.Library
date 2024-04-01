using System.Runtime.CompilerServices;

namespace Trarizon.Library.Helpers;
public static class UnsafeHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly TTo AsReadOnly<TFrom, TTo>(ref readonly TFrom source)
        => ref Unsafe.As<TFrom, TTo>(ref Unsafe.AsRef(in source));

    public static unsafe nint Offset<T>(ref readonly T origin, ref readonly T target)
    {
#pragma warning disable CS8500
        return Unsafe.ByteOffset(in origin, in target) / sizeof(T);
#pragma warning restore CS8500
    }
}
