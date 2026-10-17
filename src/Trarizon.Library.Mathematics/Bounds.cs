using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Trarizon.Library.Mathematics.Helpers;

namespace Trarizon.Library.Mathematics;

#if NET7_0_OR_GREATER

public static class BoundsExtensions
{
    extension<T>(Bounds<T> self) where T : IFloatingPoint<T>
    {
        public T Center => (self.Min + self.Max) / T.CreateChecked(2);
        public T Extents => self.Size / T.CreateChecked(2);
    }
}

public struct Bounds<T> : IEquatable<Bounds<T>> where T : INumber<T>
{
    private T _min;
    private T _max;

    public T Min => _min;
    public T Max => _max;

    public Bounds(T min, T max)
    {
        if (min > max)
            Throws.ThrowArgument("Min of bounds should less than or equals to max");
        _min = min;
        _max = max;
    }

    public readonly T Size => _max - _min;

    public readonly T Extents => Size / T.CreateChecked(2);


    public bool Contains(T value) => value >= _min && value <= _max;

    public T Clamp(T value) => _min > value ? _min : _max < value ? _max : value;
    // public T Lerp(T value) =>default;
    // public T Normalize(T value) => defualt;

    public readonly Bounds<T> Encapsulate(T value)
    {
        if (value < _min)
            return new Bounds<T>(value, _max);
        if (value > _max)
            return new Bounds<T>(_min, value);
        return this;
    }
    public readonly Bounds<T> Encapsulate(Bounds<T> other) => Encapsulate(other.Min).Encapsulate(other.Max);
    public readonly Bounds<T> Expand(T amount) => new Bounds<T>(_min - amount, _max + amount);
    public readonly Bounds<T> Offset(T amount) => new Bounds<T>(_min + amount, _max + amount);

    public static bool Overlaps(Bounds<T> left, Bounds<T> right)
        => left.Max > right.Min && left.Min < right.Max;

    public static bool Intersects(Bounds<T> left, Bounds<T> right)
        => left.Max >= right.Min && left.Min <= right.Max;

    #region Math operators

    public static bool TryIntersect(Bounds<T> left, Bounds<T> right, out Bounds<T> bounds)
    {
        if (left.Max <= right.Min || left.Min >= right.Max)
        {
            bounds = default;
            return false;
        }
        bounds = new(T.Max(left.Min, right.Min), T.Min(left.Max, right.Max));
        return true;
    }

    #endregion

    #region Equality

    public static bool operator ==(Bounds<T> left, Bounds<T> right)
        => left._min == right._min && left._max == right._max;
    public static bool operator !=(Bounds<T> left, Bounds<T> right) => !(left == right);

    public readonly bool Equals(Bounds<T> other) => this == other;
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Bounds<T> val && Equals(val);
    public override readonly int GetHashCode() => HashCode.Combine(_min, _max);

    #endregion
}

#endif
