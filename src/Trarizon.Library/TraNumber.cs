﻿using CommunityToolkit.Diagnostics;
using System.Numerics;

namespace Trarizon.Library;
public static partial class TraNumber
{
    public static bool IncAndTryWrap<T>(this ref T number, T delta, T max)
        where T : struct, INumberBase<T>, IComparisonOperators<T, T, bool>, IModulusOperators<T, T, T>
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

    public static void IncAndWrap<T>(this ref T number, T delta, T max)
        where T : struct, INumberBase<T>, IComparisonOperators<T, T, bool>, IModulusOperators<T, T, T>
    {
        number += delta;
        if (number > max) {
            number %= max;
        }
    }

    /// <summary>
    /// Linear normalize value into [0,1]
    /// </summary>
    public static T Normalize<T>(T min, T max, T value) where T : IFloatingPointIeee754<T>
    {
        if (min == max)
            return T.Zero;

        return T.Clamp((value - min) / (max - min), T.Zero, T.One);
    }

    /// <summary>
    /// Linear normalize value without clamp the result into [0, 1]
    /// <br/>
    /// eg in range [5,10], 15 result in 5, 0 result in -1
    /// </summary>
    public static T NormalizeUnclamped<T>(T min, T max, T value) where T : IFloatingPointIeee754<T>
    {
        if (min == max)
            return T.Zero;
        return (value - min) / (max - min);
    }

    /// <summary>
    /// Linear map a value from [<paramref name="fromMin"/>, <paramref name="fromMax"/>] 
    /// to [<paramref name="toMin"/>, <paramref name="toMax"/>]. The method does not clamp value
    /// </summary>
    public static T MapTo<T>(T value, T fromMin, T fromMax, T toMin, T toMax) where T : IFloatingPointIeee754<T>
    {
        var lerp = (value - fromMin) / (fromMax - fromMin);
        return (toMax - toMin) * lerp + toMin;
    }

    /// <summary>
    /// <see cref="Index.GetOffset(int)"/>, and check if the offset is in [0, <paramref name="length"/>),
    /// throw if out of range
    /// </summary>
    public static int GetCheckedOffset(this Index index, int length)
    {
        var offset = index.GetOffset(length);
        Guard.IsInRange(offset, 0, length);
        return offset;
    }

    /// <summary>
    /// <see cref="Range.GetOffsetAndLength(int)"/>, and check if the offset and count is in [0, <paramref name="length"/>),
    /// throw if out of range
    /// </summary>
    public static (int Offset, int Length) GetCheckedOffsetAndLength(this Range range, int length)
    {
        var (ofs, len) = range.GetOffsetAndLength(length);
        ArgumentOutOfRangeException.ThrowIfNegative(ofs, nameof(range));
        ArgumentOutOfRangeException.ThrowIfNegative(len, nameof(range));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(ofs + len, length, nameof(range));
        return (ofs, len);
    }
}
