using CommunityToolkit.Diagnostics;

namespace Trarizon.Library.Mathematics;
public static partial class TraMath
{
#if NETSTANDARD

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

    public static double Clamp(double value, double min, double max)
    {
        Guard.IsLessThanOrEqualTo(min, max);
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    public static float Lerp(float min, float max, float amount)
        => min + amount * (max - min);

    public static double Lerp(double min, double max, double amount)
        => min + amount * (max - min);

#endif
}
