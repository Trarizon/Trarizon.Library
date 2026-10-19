using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Trarizon.Library.Mathematics;

internal static partial class NumberHelpers
{
#if NET7_0_OR_GREATER

    public static bool IsIBinaryInteger<T>()
    {
        if (typeof(T) == typeof(int) || typeof(T) == typeof(uint) || typeof(T) == typeof(long) || typeof(T) == typeof(ulong))
            return true;
        return GenericMath<T>.IsBinaryInteger();
    }
    public static bool IsIFloatingPoint<T>() => GenericMath<T>.IsFloatingPoint();

    private static class GenericMath<T>
    {
        private static bool? _isBinaryInteger;
        private static bool? _isFloatingPoint;

        public static bool IsBinaryInteger()
            => _isBinaryInteger ??= typeof(IBinaryInteger<>).MakeGenericType(typeof(T)).IsAssignableFrom(typeof(T));

        public static bool IsFloatingPoint()
            => _isFloatingPoint ??= typeof(IFloatingPoint<>).MakeGenericType(typeof(T)).IsAssignableFrom(typeof(T));
    }

#endif
}
