// This file mirrors TraNumber.cs for .NET Standard, as abstract static method appears from .NET 7

#if NETSTANDARD

using CommunityToolkit.HighPerformance;

namespace Trarizon.Library.Common;
public static partial class TraNumber
{
    public static bool IncAndTryWrap(this ref long number, long delta, long max)
    {
        number += delta;
        if (number > max) {
            number %= max;
            return true;
        }
        else {
            return false;
        }
    }

    public static bool IncAndTryWrap(this ref ulong number, ulong delta, ulong max)
    {
        number += delta;
        if (number > max) {
            number %= max;
            return true;
        }
        else {
            return false;
        }
    }

    public static void IncAndWrap(this ref long number, long delta, long max)
    {
        number += delta;
        if (number > max) {
            number %= max;
        }
    }

    public static void IncAndWrap(this ref ulong number, ulong delta, ulong max)
    {
        number += delta;
        if (number > max) {
            number %= max;
        }
    }

    /// <summary>
    /// if (value < 0) value = ~value, this method is useful with return value of <c>Search</c>s
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    public static void FlipNegative(ref long value)
    {
        if (value < 0)
            value = ~value;
    }

    #region MinMax

    public static long Min(long v0, long v1, long v2)
        => v0 > v1 ? v1 : v0 > v2 ? v2 : v0;

    public static long Min(params ReadOnlySpan<long> values) 
    {
        var rtn = values[0];
        for (var i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val < rtn)
                rtn = val;
        }
        return rtn;
    }

    public static ulong Min(ulong v0, ulong v1, ulong v2)
        => v0 > v1 ? v1 : v0 > v2 ? v2 : v0;

    public static ulong Min(params ReadOnlySpan<ulong> values) 
    {
        var rtn = values[0];
        for (var i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val < rtn)
                rtn = val;
        }
        return rtn;
    }

    public static long Max(long v0, long v1, long v2)
        => v0 < v1 ? v1 : v0 < v2 ? v2 : v0;

    public static long Max(params ReadOnlySpan<long> values) 
    {
        var rtn = values[0];
        for (var i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val > rtn)
                rtn = val;
        }
        return rtn;
    }

    public static ulong Max(ulong v0, ulong v1, ulong v2)
        => v0 < v1 ? v1 : v0 < v2 ? v2 : v0;

    public static ulong Max(params ReadOnlySpan<ulong> values) 
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
    public static (long Min, long Max) MinMax(long left, long right)
    {
        if (left <= right)
            return (left, right);
        else
            return (right, left);
    }

    public static (long Min, long Max) MinMax(params ReadOnlySpan<long> values) 
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
    public static (ulong Min, ulong Max) MinMax(ulong left, ulong right)
    {
        if (left <= right)
            return (left, right);
        else
            return (right, left);
    }

    public static (ulong Min, ulong Max) MinMax(params ReadOnlySpan<ulong> values) 
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
}

#endif
