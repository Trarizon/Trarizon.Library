using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Trarizon.Library.Numerics;
public struct Ray2D(Vector2 origin, Vector2 direction) : IEquatable<Ray2D>
#if NET7_0_OR_GREATER
    , IEqualityOperators<Ray2D, Ray2D, bool>
#endif
{
    public Vector2 Origin = origin;
    public Vector2 Direction = direction.ToNormalized();

    public readonly Ray2D Translate(float distance)
        => new(GetPointAt(distance), Direction);

    public readonly Vector2 GetPointAt(float distance)
        => Origin + Direction * distance;

    public static bool operator ==(Ray2D l, Ray2D r) => l.Origin == r.Origin && l.Direction == r.Direction;
    public static bool operator !=(Ray2D l, Ray2D r) => !(l == r);

    public readonly bool Equals(Ray2D other) => this == other;
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Ray2D ray && Equals(ray);
    public override readonly int GetHashCode() => HashCode.Combine(Origin, Direction);
    public override readonly string ToString() => $"{{Origin:{Origin} Direction:{Direction}}}";
}
