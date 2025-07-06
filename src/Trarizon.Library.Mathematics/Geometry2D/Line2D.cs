using System.Diagnostics;
using System.Numerics;
using Trarizon.Library.Mathematics.Helpers;

namespace Trarizon.Library.Mathematics.Geometry2D;
public readonly struct Line2D
{
    // ax + by + c = 0
    private readonly float _a;
    private readonly float _b;
    private readonly float _c;

    public Line2D(float a, float b, float c)
    {
        if (a == 0 && b == 0)
            Throws.ThrowArgument("x and y cannot both have zero coefficients simultaneously.");

        _a = a;
        _b = b;
        _c = c;
    }

    public static Line2D AxisX => new Line2D(0f, 1f, 0f);
    public static Line2D AxisY => new Line2D(1f, 0f, 0f);

    public bool IsHorizontal => _a == 0f;
    public bool IsVertical => _b == 0f;

    public float XIntercept => _a != 0 ? -_c / _a : float.NaN;
    public float YIntercept => _b != 0 ? -_c / _b : float.NaN;

    public float Slope => _b != 0 ? -_a / _b : float.PositiveInfinity;

    /// <summary>
    /// If line is parallel to y axis, return <see cref="float.NaN"/>
    /// </summary>
    public float GetY(float x)
    {
        if (_b == 0)
            return float.NaN;

        return (-_a * x - _c) / _b;
    }

    /// <summary>
    /// If line is parallel to x axis, return <see cref="float.NaN"/>
    /// </summary>
    public float GetX(float y)
    {
        if (_a == 0)
            return float.NaN;
        return (-_b * y - _c) / _a;
    }

    public bool IsOnLine(float x, float y)
        => _a * x + _b * y == -_c;

    #region Create

    public static Line2D CreateSlopeIntercept(float slope, float intercept)
        => new Line2D(slope, -1, intercept);

    public static Line2D CreateLineSlope(Vector2 point, float slope)
        => new Line2D(slope, -1, point.Y - slope * point.X);

    public static Line2D CreatePoints(Vector2 p1, Vector2 p2)
    {
        if (p1 == p2)
            Throws.ThrowArgument("p1 and p2 are same point");

        if (p1.X * p2.Y == p1.Y * p2.X)
            return new Line2D(p1.Y, -p2.X, 0f);

        var n = p2.Y * p1.X - p1.Y * p2.X;
        var b = (p1.X - p2.X) / n;
        var a = (p1.Y - p2.X) / -n;
        return new Line2D(a, b, 1);
    }

    public static Line2D CreateIntercepts(float xIntercept, float yIntercept)
        => new Line2D(yIntercept, xIntercept, xIntercept * yIntercept);

    #endregion

    public static Vector2? Intersection(Line2D left, Line2D right)
    {
        float a1 = left._a, a2 = right._a;
        float b1 = left._b, b2 = right._b;
        float c1 = left._c, c2 = right._c;

        var n = a2 * b1 - a1 * b2;
        if (n == 0) {
            Debug.Assert(AreParallel(left, right));
            return null;
        }
        var x = (c1 * b2 - c2 * b1) / n;
        var y = (c1 * a2 - c2 * a1) / -n;
        return new Vector2(x, y);
    }

    public static bool AreParallel(Line2D left, Line2D right)
        => left._a * right._b == right._a * left._b;
}
