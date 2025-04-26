using CommunityToolkit.HighPerformance;
using System.Numerics;

namespace Trarizon.Library.Common;
public static partial class TraNumber
{
#if NET7_0_OR_GREATER

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
    /// if (value < 0) value = ~value, this method is useful with return value of <c>Search</c>s
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    public static void FlipNegative<T>(ref T value) where T : IBinaryInteger<T>, ISignedNumber<T>
    {
        if (value < T.Zero)
            value = ~value;
    }

#endif
}
