using CommunityToolkit.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library;
public static class TraEnum
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAnyFlag<T>(this T value, T flags) where T : struct, Enum
    {
#if NET7_0_OR_GREATER
        if (Unsafe.SizeOf<T>() == 1)
            return Cmp<byte>();
        if (Unsafe.SizeOf<T>() == 2)
            return Cmp<short>();
        if (Unsafe.SizeOf<T>() == 4)
            return Cmp<int>();
        if (Unsafe.SizeOf<T>() == 8)
            return Cmp<long>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool Cmp<TTarget>() where TTarget : struct, INumberBase<TTarget>, IBitwiseOperators<TTarget, TTarget, TTarget>
        {
            return (Unsafe.As<T, TTarget>(ref value) & Unsafe.As<T, TTarget>(ref flags)) != TTarget.Zero;
        }
#else
        if (Unsafe.SizeOf<T>() == 1)
            return (Unsafe.As<T, byte>(ref value) & Unsafe.As<T, byte>(ref flags)) != 0;
        if (Unsafe.SizeOf<T>() == 2)
            return (Unsafe.As<T, short>(ref value) & Unsafe.As<T, short>(ref flags)) != 0;
        if (Unsafe.SizeOf<T>() == 4)
            return (Unsafe.As<T, int>(ref value) & Unsafe.As<T, int>(ref flags)) != 0;
        if (Unsafe.SizeOf<T>() == 8)
            return (Unsafe.As<T, long>(ref value) & Unsafe.As<T, long>(ref flags)) != 0L;
#endif
        return ThrowHelper.ThrowNotSupportedException<bool>("Not supported enum size");
    }
}
