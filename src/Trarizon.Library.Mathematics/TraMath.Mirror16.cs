using TNum = short;

namespace Trarizon.Library.Mathematics;
public static partial class TraMath
{
#if NETSTANDARD

    #region IncAndWrap/Mod

    public static bool IncAndTryWrap(ref TNum number, TNum delta, TNum max)
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

    public static void IncAndWrap(ref TNum number, TNum delta, TNum max)
    {
        number += delta;
        if (number > max) {
            number -= max;
        }
    }

    public static bool IncAndTryMod(ref TNum number, TNum delta, TNum max)
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

    public static void IncAndMod(ref TNum number, TNum delta, TNum max)
    {
        number += delta;
        if (number > max) {
            number %= max;
        }
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
    public static (TNum Min, TNum Max) MinMax(TNum left, TNum right)
    {
        if (left <= right)
            return (left, right);
        else
            return (right, left);
    }

    #endregion

#endif
}
