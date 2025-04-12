using System.Numerics;

namespace Trarizon.Library.Mathematics.Geometry2D;
public struct LineSegment2D(Vector2 start, Vector2 end)
{
    public Vector2 StartPoint = start;
    public Vector2 EndPoint = end;

    public readonly float Length => (EndPoint - StartPoint).Length();

    public readonly Vector2 Midpoint => (EndPoint + StartPoint) / 2f;

    public readonly Vector2 Direction => EndPoint - StartPoint;
}
