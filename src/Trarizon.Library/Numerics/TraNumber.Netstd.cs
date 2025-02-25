using CommunityToolkit.Diagnostics;

#if NETSTANDARD

namespace Trarizon.Library.Numerics;
public static partial class TraNumber
{
    public static int Clamp(int value, int min, int max)
    {
        Guard.IsLessThanOrEqualTo(min, max);
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    public static float Clamp(float value, float min, float max)
    {
        Guard.IsLessThanOrEqualTo(min, max);
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    public static float Lerp(float min, float max, float amount)
        => min + amount * (max - min);
}

#endif