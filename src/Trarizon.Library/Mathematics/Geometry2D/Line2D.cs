using System.Numerics;

namespace Trarizon.Library.Mathematics.Geometry2D;
public struct Line2D(Vector2 point, Vector2 direction)
{
    private Vector2 _point = point;
    private Vector2 _dir = direction;
    public static Line2D AxisX => new(new(0, 0), new(1, 0));
    public static Line2D AxisY => new(new(0, 0), new(0, 1));


    public static Vector2? Intersection(Line2D left, Line2D right)
    {
        // Deepseek

        Vector2 p1 = left._point;
        Vector2 d1 = left._dir;
        Vector2 p2 = right._point;
        Vector2 d2 = right._dir;

        var denominator = d1.X * d2.Y - d1.Y * d2.X;

        if (Math.Abs(denominator) < float.Epsilon) {
            return default;
        }

        float t = ((p2.X - p1.X) * d2.Y - (p2.Y - p1.Y) * d2.X) / denominator;

        // 计算交点
        return p1 + t * d1;
    }
}
