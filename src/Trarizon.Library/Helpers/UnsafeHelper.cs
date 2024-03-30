using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Helpers;
public static class UnsafeHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]  
    public static ref readonly TTo AsReadOnly<TFrom, TTo>(ref readonly TFrom source)
        => ref Unsafe.As<TFrom, TTo>(ref Unsafe.AsRef(in source));

    internal static unsafe int SubstractRef<T>(ref readonly T left, ref readonly T right)
    {
#pragma warning disable CS8500
        fixed (T* rPtr = &right, lPtr = &left) {
            var res = (int)((nuint)lPtr - (nuint)rPtr) / sizeof(T);
            Debug.Assert(Unsafe.AreSame(in Unsafe.Subtract(ref Unsafe.AsRef(in left), res), in right));
            return res;
        }
#pragma warning restore CS8500
    }
}
