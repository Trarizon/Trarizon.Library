using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Trarizon.Library.Numerics;
public struct Ray3D(Vector3 origin, Vector3 direction) : IEquatable<Ray3D>, IEqualityOperators<Ray3D, Ray3D, bool>
{
    /// <summary>
    /// Origin point of the ray
    /// </summary>
    public Vector3 Origin = origin;
    /// <summary>
    /// Direction of the ray, the vector is normalized
    /// </summary>
    public Vector3 Direction = direction.ToNormalized();

    public readonly Ray3D Translate(float distance)
        => new(GetPointAt(distance), Direction);

    public readonly Vector3 GetPointAt(float distance)
        => Origin + Direction * distance;

    public static bool operator ==(Ray3D l, Ray3D r) => l.Origin == r.Origin && l.Direction == r.Direction;
    public static bool operator !=(Ray3D l, Ray3D r) => !(l == r);

    public readonly bool Equals(Ray3D other) => this == other;
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Ray3D ray && Equals(ray);
    public override readonly int GetHashCode() => HashCode.Combine(Origin, Direction);
    public override readonly string ToString() => $"{{Origin:{Origin} Direction:{Direction}}}";
}
