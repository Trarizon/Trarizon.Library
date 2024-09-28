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
}
