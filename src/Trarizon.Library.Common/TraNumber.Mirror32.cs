// This file mirrors TraNumber.cs for .NET Standard, as abstract static method appears from .NET 7

#if NETSTANDARD

using CommunityToolkit.HighPerformance;

namespace Trarizon.Library.Common;
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

}

#endif
