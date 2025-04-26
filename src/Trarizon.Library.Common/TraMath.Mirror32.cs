using CommunityToolkit.HighPerformance;
using TNum = int;
using TUNum = uint;

namespace Trarizon.Library.Mathematics;
public static partial class TraMath
{
#if NETSTANDARD

    public static TNum GreatestCommonDivisor(TNum left, TNum right)
    {
        while (right != 0) {
            var tmp = left;
            left = right;
            right = tmp % right;
        }
        return left;
    }

    public static TNum LeastCommonMultiple(TNum left, TNum right)
        => left * right / GreatestCommonDivisor(left, right);

    #region MinMax

    public static TNum Min(TNum v0, TNum v1, TNum v2)
    => v0 > v1 ? v1 : v0 > v2 ? v2 : v0;

    public static TNum Min(params ReadOnlySpan<TNum> values)
    {
        var rtn = values[0];
        for (var i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val < rtn)
                rtn = val;
        }
        return rtn;
    }
    public static TUNum Min(TUNum v0, TUNum v1, TUNum v2)
    => v0 > v1 ? v1 : v0 > v2 ? v2 : v0;

    public static TUNum Min(params ReadOnlySpan<TUNum> values)
    {
        var rtn = values[0];
        for (var i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val < rtn)
                rtn = val;
        }
        return rtn;
    }
    public static TNum Max(TNum v0, TNum v1, TNum v2)
    => v0 < v1 ? v1 : v0 < v2 ? v2 : v0;

    public static TNum Max(params ReadOnlySpan<TNum> values)
    {
        var rtn = values[0];
        for (var i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val > rtn)
                rtn = val;
        }
        return rtn;
    }
    public static TUNum Max(TUNum v0, TUNum v1, TUNum v2)
    => v0 < v1 ? v1 : v0 < v2 ? v2 : v0;

    public static TUNum Max(params ReadOnlySpan<TUNum> values)
    {
        var rtn = values[0];
        for (var i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val > rtn)
                rtn = val;
        }
        return rtn;
    }

    /// <summary>
    /// Returns min, max in one time
    /// </summary>
    /// <returns>
    /// If <paramref name="left"/> equals <paramref name="right"/>, the return value is (<paramref name="left"/>, <paramref name="right"/>),
    /// else Min is the less one
    /// </returns>
    public static (TNum Min, TNum Max) MinMax(TNum left, TNum right)
    {
        if (left <= right)
            return (left, right);
        else
            return (right, left);
    }

    public static (TNum Min, TNum Max) MinMax(params ReadOnlySpan<TNum> values)
    {
        var min = values[0];
        var max = min;
        for (var i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val < min)
                min = val;
            else if (val > max)
                max = val;
        }
        return (min, max);
    }

    /// <summary>
    /// Returns min, max in one time
    /// </summary>
    /// <returns>
    /// If <paramref name="left"/> equals <paramref name="right"/>, the return value is (<paramref name="left"/>, <paramref name="right"/>),
    /// else Min is the less one
    /// </returns>
    public static (TUNum Min, TUNum Max) MinMax(TUNum left, TUNum right)
    {
        if (left <= right)
            return (left, right);
        else
            return (right, left);
    }

    public static (TUNum Min, TUNum Max) MinMax(params ReadOnlySpan<TUNum> values)
    {
        var min = values[0];
        var max = min;
        for (var i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val < min)
                min = val;
            else if (val > max)
                max = val;
        }
        return (min, max);
    }

    #endregion

#endif
}
