using TNum = double;

namespace Trarizon.Library.Mathematics;
public static partial class TraMath
{
#if NETSTANDARD

    #region Map

    /// <summary>
    /// Linear normalize value into [0,1]
    /// </summary>
    public static TNum Normalize(TNum min, TNum max, TNum value)
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
    public static TNum NormalizeUnclamped(TNum min, TNum max, TNum value)
    {
        if (min == max)
            return 0;
        return (value - min) / (max - min);
    }

    /// <summary>
    /// Linear map a value from [<paramref name="fromMin"/>, <paramref name="fromMax"/>] 
    /// to [<paramref name="toMin"/>, <paramref name="toMax"/>]. The method does not clamp value
    /// </summary>
    public static TNum MapTo(TNum value, TNum fromMin, TNum fromMax, TNum toMin, TNum toMax)
    {
        var lerp = (value - fromMin) / (fromMax - fromMin);
        return (toMax - toMin) * lerp + toMin;
    }

    public static TNum MapToClamped(TNum value, TNum fromMin, TNum fromMax, TNum toMin, TNum toMax)
    {
        var lerp = (value - fromMin) / (fromMax - fromMin);
        return Clamp((toMax - toMin) * lerp + toMin, toMin, toMax);
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
