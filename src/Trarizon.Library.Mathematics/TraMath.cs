using System.Numerics;

namespace Trarizon.Library.Mathematics;
public static partial class TraMath
{
#if NET7_0_OR_GREATER

    #region GCD / LCM

    public static T GreatestCommonDivisor<T>(T left, T right) where T : IBinaryInteger<T>
    {
        while (right != T.Zero) {
            var tmp = left;
            left = right;
            right = tmp % right;
        }
        return left;
    }

    public static T LeastCommonMultiple<T>(T left, T right) where T : IBinaryInteger<T>
        => left * right / GreatestCommonDivisor(left, right);

    #endregion

    #region IncAndWrap/Mod

    public static bool IncAndTryWrap<T>(ref T number, T delta, T max) where T : struct, INumber<T>
    {
        number += delta;
        if (number > max) {
            number -= max;
            return true;
        }
        else {
            return false;
        }
    }

    public static void IncAndWrap<T>(ref T number, T delta, T max) where T : struct, INumber<T>
    {
        number += delta;
        if (number > max) {
            number -= max;
        }
    }

    public static bool IncAndTryMod<T>(ref T number, T delta, T max) where T : struct, INumber<T>
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

    public static void IncAndMod<T>(ref T number, T delta, T max) where T : struct, INumber<T>
    {
        number += delta;
        if (number > max) {
            number %= max;
        }
    }


    #endregion

    #region Map

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

    public static T MapToClamped<T>(T value, T fromMin, T fromMax, T toMin, T toMax) where T : IFloatingPointIeee754<T>
    {
        var lerp = (value - fromMin) / (fromMax - fromMin);
        return T.Clamp((toMax - toMin) * lerp + toMin, toMin, toMax);
    }

    #endregion

    #region MinMax

    /// <summary>
    /// Returns min, max in one time
    /// </summary>
    /// <returns>
    /// If <paramref name="left"/> equals <paramref name="right"/>, the return value is (<paramref name="left"/>, <paramref name="right"/>),
    /// else Min is the less one
    /// </returns>
    public static (T Min, T Max) MinMax<T>(T left, T right) where T : INumber<T>
    {
        if (left <= right)
            return (left, right);
        else
            return (right, left);
    }

    #endregion

#endif
}
