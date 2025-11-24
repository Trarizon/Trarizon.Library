using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Functional.Internal;

internal static class BoxHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object? Box<T>(T? value)
    {
        // For reference type, return itself
        if (!typeof(T).IsValueType)
            return value;

        // Nullable<T> null, return default box;
        if (value is null)
            return DefaultBoxes<T>.Instance;

        // For boolean, return cached box
        if (typeof(T) == typeof(bool)) {
            var val = Unsafe.As<T, bool>(ref value!);
            return val ? BooleanBoxes.True : DefaultBoxes<bool>.Instance;
        }

#if NETSTANDARD2_0
        if (typeof(T) == typeof(int)) {
            var val = Unsafe.As<T, int>(ref value!);
            return Int32Boxes.Get(val);
        }
#else
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
            if (Unsafe.SizeOf<T>() == sizeof(byte) && Unsafe.As<T, byte>(ref value) == default(byte))
                return DefaultBoxes<byte>.Instance;
            if (Unsafe.SizeOf<T>() == sizeof(short) && Unsafe.As<T, short>(ref value) == default(short))
                return DefaultBoxes<short>.Instance;
            if (Unsafe.SizeOf<T>() == sizeof(int))
                return Int32Boxes.Get(Unsafe.As<T, int>(ref value));
            if (Unsafe.SizeOf<T>() == sizeof(long) && Unsafe.As<T, long>(ref value) == default(long))
                return DefaultBoxes<long>.Instance;
        }
#endif
        return new ValueBox<T>(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly T? UnboxRef<T>(ref readonly object? value)
    {
        if (!typeof(T).IsValueType) {
            Debug.Assert(value is T or null);
            return ref Unsafe.As<object?, T?>(ref Unsafe.AsRef(in value));
        }

        if (value is null)
            return ref UnboxValueTypeDefaultRef<T>();
        return ref Unsafe.As<object, ValueBox<T>>(ref Unsafe.AsRef(in value)).Value!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? Unbox<T>(object? value)
    {
        if (!typeof(T).IsValueType) {
            Debug.Assert(value is T or null);
            return Unsafe.As<object?, T?>(ref value);
        }

        if (value is null)
            return default;
        return Unsafe.As<ValueBox<T>>(value).Value;
    }

    public static bool IsValidBox<T>(object? obj)
    {
        if (obj is null)
            return true;
        if (!typeof(T).IsValueType)
            return true;

        if (typeof(T) == typeof(bool))
            return obj == DefaultBoxes<bool>.Instance || obj == BooleanBoxes.True;

        if (default(T) is null)
            return obj is ValueBox<T>;

#if NETSTANDARD2_0
        return obj is ValueBox<T>;
#else
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
            if (Unsafe.SizeOf<T>() == sizeof(byte) && obj is ValueBox<byte>)
                return true;
            if (Unsafe.SizeOf<T>() == sizeof(short) && obj is ValueBox<short>)
                return true;
            if (Unsafe.SizeOf<T>() == sizeof(int) && obj is ValueBox<int>)
                return true;
            if (Unsafe.SizeOf<T>() == sizeof(long) && obj is ValueBox<long>)
                return true;
        }
        return obj is ValueBox<T>;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref readonly T? UnboxValueTypeDefaultRef<T>()
    {
        Debug.Assert(typeof(T).IsValueType);
        if (default(T) is null)
            return ref DefaultBoxes<T>.Instance.Value!;

        if (typeof(T) == typeof(bool))
            return ref Unsafe.As<bool, T>(ref Unsafe.AsRef(in DefaultBoxes<bool>.Instance.Value))!;
#if NETSTANDARD2_0
        if (typeof(T) == typeof(int))
            return ref Unsafe.As<int, T>(ref Unsafe.AsRef(in Int32Boxes.Default.Value))!;
#else
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
            if (Unsafe.SizeOf<T>() == sizeof(byte))
                return ref Unsafe.As<byte, T>(ref Unsafe.AsRef(in DefaultBoxes<byte>.Instance.Value))!;
            if (Unsafe.SizeOf<T>() == sizeof(short))
                return ref Unsafe.As<short, T>(ref Unsafe.AsRef(in DefaultBoxes<short>.Instance.Value))!;
            if (Unsafe.SizeOf<T>() == sizeof(int))
                return ref Unsafe.As<int, T>(ref Unsafe.AsRef(in Int32Boxes.Default.Value))!;
            if (Unsafe.SizeOf<T>() == sizeof(long))
                return ref Unsafe.As<long, T>(ref Unsafe.AsRef(in DefaultBoxes<long>.Instance.Value))!;
        }
#endif
        return ref DefaultBoxes<T>.Instance.Value!;
    }

    private static class DefaultBoxes<T>
    {
        public static readonly ValueBox<T> Instance = new(default!);
    }

    private static class BooleanBoxes
    {
        public static readonly ValueBox<bool> True = new(true);
    }

    private static class Int32Boxes
    {
        private const int Min = -16;
        private const int Max = 16;

        private static readonly ValueBox<int>[] _cache = Create();

        public static ref readonly ValueBox<int> Default => ref _cache[0 - Min];

        public static ValueBox<int> Get(int value)
        {
            var idx = value - Min;
            if ((uint)idx < Max - Min) {
                return _cache[idx];
            }
            return new ValueBox<int>(value);
        }

        private static ValueBox<int>[] Create()
        {
            var cache = new ValueBox<int>[Max - Min];
            for (int i = 0; i < Max - Min; i++)
                cache[i] = new ValueBox<int>(i + Min);
            return cache;
        }
    }
}

internal sealed class ValueBox<T>(T value)
{
    public readonly T Value = value;
}
