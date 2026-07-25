using System.Numerics;

namespace Trarizon.Library;

public static partial class TraMath
{
#if NET7_0_OR_GREATER

    extension<T>(T) where T : IFloatingPointIeee754<T>
    {
        public static T Normalize(T min, T max, T value)
            => min == max ? T.Zero : T.Clamp((value - min) / (max - min), T.Zero, T.One);

        public static T NormalizeUnclamped(T min, T max, T value)
            => min == max ? T.Zero : (value - min) / (max - min);
    
        public static T Remap(T value, T fromMin, T fromMax, T toMin, T toMax)
            => (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }

#else

    extension(float)
    {
        public static float Normalize(float min, float max, float value)
            => min == max ? 0f : float.Clamp((value - min) / (max - min), 0f, 1f);

        public static float NormalizeUnclamped(float min, float max, float value)
            => min == max ? 0f : (value - min) / (max - min);

        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
            => (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }

    extension(double)
    {
        public static double Normalize(double min, double max, double value)
            => min == max ? 0 : double.Clamp((value - min) / (max - min), 0, 1);

        public static double NormalizeUnclamped(double min, double max, double value)
            => min == max ? 0 : (value - min) / (max - min);

        public static double Remap(double value, double fromMin, double fromMax, double toMin, double toMax)
            => (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }


#endif

#if NET7_0_OR_GREATER

    extension<T>(T) where T : INumber<T>
    {
        public static void Sort(ref T left, ref T right)
        {
            if (left > right)
                (left, right) = (right, left);
        }

        public static (T Min, T Max) Sort(T left, T right)
            => left > right ? (right, left) : (left, right);
    }
    
#else

    extension(int)
    {
        public static (int Min, int Max) Sort(int left, int right)
            => left > right ? (right, left) : (left, right);

        public static void Sort(ref int left, ref int right)
        {
            if (left > right)
                (left, right) = (right, left);
        }
    }

#endif
}