#if NETSTANDARD
#pragma warning disable CS8604 // 引用类型参数可能为 null。
#endif

using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;
public static partial class TraComparison
{
    internal readonly struct ComparerEquatable<T, TComparer>(T value, TComparer comparer) : IEquatable<T> where TComparer : IEqualityComparer<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(T? other) => comparer.Equals(value, other);
    }

    internal readonly struct DefaultComparerEquatable<T>(T value) : IEquatable<T>, IEquatable<DefaultComparerEquatable<T>>
    {
        private readonly T _value = value;

        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(T? other) => EqualityComparer<T>.Default.Equals(_value, other);

        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(DefaultComparerEquatable<T> other) => EqualityComparer<T>.Default.Equals(_value, other._value);

        public override bool Equals(object? obj) => obj is DefaultComparerEquatable<T> other && Equals(other);

        public override int GetHashCode() => _value?.GetHashCode() ?? 0;
    }
}
