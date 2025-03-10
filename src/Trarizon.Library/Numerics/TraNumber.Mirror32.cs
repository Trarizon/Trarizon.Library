﻿// This file mirrors TraNumber.cs for .NET Standard, as abstract static method appears from .NET 7

#if NETSTANDARD

using CommunityToolkit.HighPerformance;

namespace Trarizon.Library.Numerics;
public static partial class TraNumber
{
    public static bool IncAndTryWrap(this ref int number, int delta, int max)
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

    public static bool IncAndTryWrap(this ref uint number, uint delta, uint max)
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

    public static void IncAndWrap(this ref int number, int delta, int max)
    {
        number += delta;
        if (number > max) {
            number %= max;
        }
    }

    public static void IncAndWrap(this ref uint number, uint delta, uint max)
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
    public static void FlipNegative(ref int value)
    {
        if (value < 0)
            value = ~value;
    }

    #region MinMax

    public static int Min(int v0, int v1, int v2)
        => v0 > v1 ? v1 : v0 > v2 ? v2 : v0;

    public static int Min(params ReadOnlySpan<int> values) 
    {
        var rtn = values[0];
        for (var i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val < rtn)
                rtn = val;
        }
        return rtn;
    }

    public static uint Min(uint v0, uint v1, uint v2)
        => v0 > v1 ? v1 : v0 > v2 ? v2 : v0;

    public static uint Min(params ReadOnlySpan<uint> values) 
    {
        var rtn = values[0];
        for (var i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val < rtn)
                rtn = val;
        }
        return rtn;
    }

    public static int Max(int v0, int v1, int v2)
        => v0 < v1 ? v1 : v0 < v2 ? v2 : v0;

    public static int Max(params ReadOnlySpan<int> values) 
    {
        var rtn = values[0];
        for (var i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val > rtn)
                rtn = val;
        }
        return rtn;
    }

    public static uint Max(uint v0, uint v1, uint v2)
        => v0 < v1 ? v1 : v0 < v2 ? v2 : v0;

    public static uint Max(params ReadOnlySpan<uint> values) 
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
    public static (int Min, int Max) MinMax(int left, int right)
    {
        if (left <= right)
            return (left, right);
        else
            return (right, left);
    }

    public static (int Min, int Max) MinMax(params ReadOnlySpan<int> values) 
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
    public static (uint Min, uint Max) MinMax(uint left, uint right)
    {
        if (left <= right)
            return (left, right);
        else
            return (right, left);
    }

    public static (uint Min, uint Max) MinMax(params ReadOnlySpan<uint> values) 
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
