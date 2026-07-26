namespace Trarizon.Library;

#if NETSTANDARD

internal static partial class Polyfills
{
    extension(float)
    {
        public static float Lerp(float value1, float value2, float amount)
            => value1 + (value2 - value1) * amount;

        public static float Clamp(float value, float min, float max)
#if NETSTANDARD2_1
            => MathF.Min(MathF.Max(value, min), max);
#else
            => value < min ? min : value > max ? max : value;
#endif
    }

    extension(double)
    {
        public static double Lerp(double value1, double value2, double amount)
            => value1 + (value2 - value1) * amount;

        public static double Clamp(double value, double min, double max)
            => Math.Min(Math.Max(value, min), max);
    }
}

#endif
