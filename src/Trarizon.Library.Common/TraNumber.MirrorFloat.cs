// This file mirrors TraNumber.cs, for .NET Standard as abstract static method appears from .NET 7

#if NETSTANDARD

using CommunityToolkit.HighPerformance;

namespace Trarizon.Library.Common;
public static partial class TraNumber
{
    /// <summary>
    /// Linear normalize value into [0,1]
    /// </summary>
    public static float Normalize(float min, float max, float value)
    {
        if (min == max)
            return 0;

        return Clamp((value - min) / (max - min), 0f, 1f);
    }

    /// <summary>
    /// Linear normalize value into [0,1]
    /// </summary>
    public static double Normalize(double min, double max, double value)
    {
        if (min == max)
            return 0;

        return Clamp((value - min) / (max - min), 0f, 1f);
    }

    /// <summary>
    /// Linear normalize value without clamp the result into [0, 1]
    /// <br/>
    /// eg in range [5,10], 15 result in 5, 0 result in -1
    /// </summary>
    public static float NormalizeUnclamped(float min, float max, float value)
    {
        if (min == max)
            return 0;
        return (value - min) / (max - min);
    }

    /// <summary>
    /// Linear normalize value without clamp the result into [0, 1]
    /// <br/>
    /// eg in range [5,10], 15 result in 5, 0 result in -1
    /// </summary>
    public static double NormalizeUnclamped(double min, double max, double value)
    {
        if (min == max)
            return 0;
        return (value - min) / (max - min);
    }

    /// <summary>
    /// Linear map a value from [<paramref name="fromMin"/>, <paramref name="fromMax"/>] 
    /// to [<paramref name="toMin"/>, <paramref name="toMax"/>]. The method does not clamp value
    /// </summary>
    public static float MapTo(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        var lerp = (value - fromMin) / (fromMax - fromMin);
        return (toMax - toMin) * lerp + toMin;
    }

    /// <summary>
    /// Linear map a value from [<paramref name="fromMin"/>, <paramref name="fromMax"/>] 
    /// to [<paramref name="toMin"/>, <paramref name="toMax"/>]. The method does not clamp value
    /// </summary>
    public static double MapTo(double value, double fromMin, double fromMax, double toMin, double toMax)
    {
        var lerp = (value - fromMin) / (fromMax - fromMin);
        return (toMax - toMin) * lerp + toMin;
    }

    public static float MapToClamped(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        var lerp = (value - fromMin) / (fromMax - fromMin);
        return Clamp((toMax - toMin) * lerp + toMin, toMin, toMax);
    }

    public static double MapToClamped(double value, double fromMin, double fromMax, double toMin, double toMax)
    {
        var lerp = (value - fromMin) / (fromMax - fromMin);
        return Clamp((toMax - toMin) * lerp + toMin, toMin, toMax);
    }

    #region MinMax

    public static float Min(float v0, float v1, float v2)
        => v0 > v1 ? v1 : v0 > v2 ? v2 : v0;

    public static float Min(params ReadOnlySpan<float> values)
    {
        var rtn = values[0];
        for (int i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val < rtn)
                rtn = val;
        }
        return rtn;
    }

    public static double Min(double v0, double v1, double v2)
        => v0 > v1 ? v1 : v0 > v2 ? v2 : v0;

    public static double Min(params ReadOnlySpan<double> values)
    {
        var rtn = values[0];
        for (int i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val < rtn)
                rtn = val;
        }
        return rtn;
    }

    public static float Max(float v0, float v1, float v2)
        => v0 < v1 ? v1 : v0 < v2 ? v2 : v0;

    public static float Max(params ReadOnlySpan<float> values)
    {
        var rtn = values[0];
        for (int i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val > rtn)
                rtn = val;
        }
        return rtn;
    }

    public static double Max(double v0, double v1, double v2)
        => v0 < v1 ? v1 : v0 < v2 ? v2 : v0;

    public static double Max(params ReadOnlySpan<double> values)
    {
        var rtn = values[0];
        for (int i = 1; i < values.Length; i++) {
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
    public static (float Min, float Max) MinMax(float left, float right)
    {
        if (left <= right)
            return (left, right);
        else
            return (right, left);
    }

    public static (float Min, float Max) MinMax(params ReadOnlySpan<float> values)
    {
        var min = values[0];
        var max = min;
        for (int i = 1; i < values.Length; i++) {
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
    public static (double Min, double Max) MinMax(double left, double right)
    {
        if (left <= right)
            return (left, right);
        else
            return (right, left);
    }

    public static (double Min, double Max) MinMax(params ReadOnlySpan<double> values)
    {
        var min = values[0];
        var max = min;
        for (int i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val < min)
                min = val;
            else if (val > max)
                max = val;
        }
        return (min, max);
    }

    #endregion
}

#endif
