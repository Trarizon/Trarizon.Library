// This file mirrors TraNumber.cs for .NET Standard, as abstract static method appears from .NET 7

#if NETSTANDARD

using CommunityToolkit.HighPerformance;

namespace Trarizon.Library.Numerics;
public static partial class TraNumber
{
    public static bool IncAndTryWrap(this ref short number, short delta, short max)
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

    public static bool IncAndTryWrap(this ref ushort number, ushort delta, ushort max)
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

    public static void IncAndWrap(this ref short number, short delta, short max)
    {
        number += delta;
        if (number > max) {
            number %= max;
        }
    }

    public static void IncAndWrap(this ref ushort number, ushort delta, ushort max)
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
    public static void FlipNegative(ref short value)
    {
        if (value < 0)
            value = (short)~value;
    }

    #region MinMax

    public static short Min(short v0, short v1, short v2)
        => v0 > v1 ? v1 : v0 > v2 ? v2 : v0;

    public static short Min(params ReadOnlySpan<short> values) 
    {
        var rtn = values[0];
        for (var i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val < rtn)
                rtn = val;
        }
        return rtn;
    }

    public static ushort Min(ushort v0, ushort v1, ushort v2)
        => v0 > v1 ? v1 : v0 > v2 ? v2 : v0;

    public static ushort Min(params ReadOnlySpan<ushort> values) 
    {
        var rtn = values[0];
        for (var i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val < rtn)
                rtn = val;
        }
        return rtn;
    }

    public static short Max(short v0, short v1, short v2)
        => v0 < v1 ? v1 : v0 < v2 ? v2 : v0;

    public static short Max(params ReadOnlySpan<short> values) 
    {
        var rtn = values[0];
        for (var i = 1; i < values.Length; i++) {
            var val = values.DangerousGetReferenceAt(i);
            if (val > rtn)
                rtn = val;
        }
        return rtn;
    }

    public static ushort Max(ushort v0, ushort v1, ushort v2)
        => v0 < v1 ? v1 : v0 < v2 ? v2 : v0;

    public static ushort Max(params ReadOnlySpan<ushort> values) 
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
    public static (short Min, short Max) MinMax(short left, short right)
    {
        if (left <= right)
            return (left, right);
        else
            return (right, left);
    }

    public static (short Min, short Max) MinMax(params ReadOnlySpan<short> values) 
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
    public static (ushort Min, ushort Max) MinMax(ushort left, ushort right)
    {
        if (left <= right)
            return (left, right);
        else
            return (right, left);
    }

    public static (ushort Min, ushort Max) MinMax(params ReadOnlySpan<ushort> values) 
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
