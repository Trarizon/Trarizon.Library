using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Functional.Internal;
internal static class ValueTypeBox
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object? BoxIfValueType<T>(T? value)
    {
        if (!typeof(T).IsValueType) {
            return value;
        }

        // Nullable<T>
        if (value is null) {
            return BoxesDefault<T>.Box;
        }
        if (typeof(T) == typeof(bool)) {
            var val = Unsafe.As<T, bool>(ref value!);
            return val ? BoxesBoolean.True : BoxesBoolean.False;
        }
#if NETSTANDARD2_0
        if (typeof(T) == typeof(int)) {
            var val = Unsafe.As<T, int>(ref value!);
            return Boxes32.Get(val);
        }
#else
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
            if (Unsafe.SizeOf<T>() == sizeof(byte) && Unsafe.As<T, byte>(ref value) == default(byte))
                return BoxesDefault<byte>.Box;
            if (Unsafe.SizeOf<T>() == sizeof(short) && Unsafe.As<T, short>(ref value) == default(short))
                return BoxesDefault<short>.Box;
            if (Unsafe.SizeOf<T>() == sizeof(int))
                return Boxes32.Get(Unsafe.As<T, int>(ref value));
            if (Unsafe.SizeOf<T>() == sizeof(long) && Unsafe.As<T, long>(ref value) == default(long))
                return BoxesDefault<long>.Box;
        }
#endif
        return new ValueTypeBox<T>(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? GetValueOrDefault<T>(object? value)
    {
        if (typeof(T).IsValueType) {
            if (value is null)
                return default;

            Debug.Assert(value is ValueTypeBox<T>);
            var box = Unsafe.As<ValueTypeBox<T>>(value);
            return box.Value;
        }
        else {
            Debug.Assert(value is T or null);
            return Unsafe.As<object?, T?>(ref value);
        }
    }

    public static bool IsValueTypeBoxOrReferenceObject<T>(object? obj)
    {
        if (obj is null)
            return true;
        if (!typeof(T).IsValueType)
            return true;

#if NETSTANDARD2_0
        return obj is ValueTypeBox<T>;
#else
        if (obj is ValueTypeBox<T>)
            return true;
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
            if (Unsafe.SizeOf<T>() == sizeof(byte) && obj is ValueTypeBox<byte>)
                return true;
            if (Unsafe.SizeOf<T>() == sizeof(short) && obj is ValueTypeBox<short>)
                return true;
            if (Unsafe.SizeOf<T>() == sizeof(int) && obj is ValueTypeBox<int>)
                return true;
            if (Unsafe.SizeOf<T>() == sizeof(long) && obj is ValueTypeBox<long>)
                return true;
        }
        return false;
#endif
    }

    private static class BoxesDefault<T>
    {
        public static readonly ValueTypeBox<T> Box = new(default!);
    }

    private static class BoxesBoolean
    {
        public static readonly ValueTypeBox<bool> True = new(true);
        public static readonly ValueTypeBox<bool> False = new(false);
    }

    private static class Boxes32
    {
        private const int Min = -16;
        private const int Max = 16;

        private static readonly ValueTypeBox<int>?[] _cache = new ValueTypeBox<int>[Max - Min];

        public static ValueTypeBox<int> Get(int value)
        {
            if (value < Min || value >= Max) {
                return new ValueTypeBox<int>(value);
            }
            return _cache[value - Min] ??= new ValueTypeBox<int>(value);
        }
    }
}

internal sealed class ValueTypeBox<T>(T value)
{
    public readonly T Value = value;
}