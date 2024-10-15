using CommunityToolkit.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library;
public static class TraEnum
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAnyFlag<T>(this T value, T flags) where T : struct, Enum
    {
        if (Unsafe.SizeOf<T>() == 1)
            return Cmp<byte>();
        if (Unsafe.SizeOf<T>() == 2)
            return Cmp<short>();
        if (Unsafe.SizeOf<T>() == 4)
            return Cmp<int>();
        if (Unsafe.SizeOf<T>() == 8)
            return Cmp<long>();
        return ThrowHelper.ThrowNotSupportedException<bool>("Not supported enum size");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool Cmp<TTarget>() where TTarget : struct, INumberBase<TTarget>, IBitwiseOperators<TTarget, TTarget, TTarget>
        {
            return (Unsafe.BitCast<T, TTarget>(value) & Unsafe.BitCast<T, TTarget>(flags)) != TTarget.Zero;
        }
    }
}
