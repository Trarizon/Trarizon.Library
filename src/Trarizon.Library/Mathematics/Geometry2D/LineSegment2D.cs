using System.Numerics;

namespace Trarizon.Library.Mathematics.Geometry2D;
public struct LineSegment2D(Vector2 start, Vector2 end)
{
    public Vector2 StartPoint = start;
    public Vector2 EndPoint = end;

    public readonly float Length => (EndPoint - StartPoint).Length();

    public readonly Vector2 Midpoint => (EndPoint + StartPoint) / 2f;

    public readonly Vector2 Direction => EndPoint - StartPoint;

    public readonly Line2D GetLine() => Line2D.CreatePoints(StartPoint, EndPoint);

    public static Line2D GetPerpendicularBisector(LineSegment2D segment)
    {
        var mid = segment.Midpoint;
        var delta = segment.Direction;
        delta = TraGeometry.RotateBy90Degree(delta, 1);
        return Line2D.CreatePoints(mid, mid + delta);
    }
}
