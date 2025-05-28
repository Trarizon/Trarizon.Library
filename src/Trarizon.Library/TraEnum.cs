using CommunityToolkit.Diagnostics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library;
public static class TraEnum
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAnyFlag<T>(this T value, T flags) where T : struct, Enum
    {
        if (Unsafe.SizeOf<T>() == 1)
            return (Unsafe.As<T, byte>(ref value) & Unsafe.As<T, byte>(ref flags)) != 0;
        if (Unsafe.SizeOf<T>() == 2)
            return (Unsafe.As<T, short>(ref value) & Unsafe.As<T, short>(ref flags)) != 0;
        if (Unsafe.SizeOf<T>() == 4)
            return (Unsafe.As<T, int>(ref value) & Unsafe.As<T, int>(ref flags)) != 0;
        if (Unsafe.SizeOf<T>() == 8)
            return (Unsafe.As<T, long>(ref value) & Unsafe.As<T, long>(ref flags)) != 0L;
        return ThrowHelper.ThrowNotSupportedException<bool>("Not supported enum size");
    }
}
