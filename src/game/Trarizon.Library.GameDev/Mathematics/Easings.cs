namespace Trarizon.Library.GameDev.Mathematics;
public static class Easings
{
    public static float SineIn(float x) => 1 - MathF.Cos(x * (MathF.PI / 2));
    public static float SineOut(float x) => MathF.Sin(x * (MathF.PI / 2));
    public static float SineInOut(float x) => -(MathF.Cos(MathF.PI * x) - 1) / 2;

    public static float QuadIn(float x) => x * x;
    public static float QuadOut(float x)
    {
        float t = 1 - x;
        return 1 - t * t;
    }

    public static float QuadInOut(float x)
    {
        if (x < 0.5f)
            return 2 * x * x;
        else {
            float t = -2 * x + 2;
            return 1 - t * t / 2;
        }
    }

    public static float CubicIn(float x) => x * x * x;
    public static float CubicOut(float x)
    {
        float t = 1 - x;
        return 1 - t * t * t;
    }
    public static float CubicInOut(float x)
    {
        if (x < 0.5f)
            return 4 * x * x * x;
        else {
            float t = -2 * x + 2;
            return 1 - t * t * t / 2;
        }
    }

    public static float QuartIn(float x) => x * x * x * x;
    public static float QuartOut(float x)
    {
        float t = 1 - x;
        return 1 - t * t * t * t;
    }
    public static float QuartInOut(float x)
    {
        if (x < 0.5f)
            return 8 * x * x * x * x;
        else {
            float t = -2 * x + 2;
            return 1 - t * t * t * t / 2;
        }
    }

    public static float QuintIn(float x) => x * x * x * x * x;
    public static float QuintOut(float x)
    {
        float t = 1 - x;
        return 1 - t * t * t * t * t;
    }
    public static float QuintInOut(float x)
    {
        if (x < 0.5f)
            return 16 * x * x * x * x * x;
        else {
            float t = -2 * x + 2;
            return 1 - t * t * t * t * t / 2;
        }
    }

    public static float ExpoIn(float x) => x == 0f ? 0f : MathF.Pow(2f, 10f * x - 10);
    public static float ExpoOut(float x) => x == 1f ? 1 : 1 - MathF.Pow(2, -10 * x);
    public static float ExpoInOut(float x)
    {
        if (x is 0f or 1f)
            return x;
        if (x < 0.5f)
            return MathF.Pow(2, 20 * x - 10) / 2;
        else
            return (2 - MathF.Pow(2, -20 * x + 10)) / 2;
    }

    public static float CircIn(float x) => 1 - MathF.Sqrt(1 - x * x);
    public static float CircOut(float x)
    {
        float t = x - 1;
        return MathF.Sqrt(1 - t * t);
    }
    public static float CircInOut(float x)
    {
        if (x < 0.5f) {
            float t = 2 * x;
            return (1 - MathF.Sqrt(1 - t * t)) / 2;
        }
        else {
            float t = -2 * x + 2;
            return (MathF.Sqrt(1 - t * t) + 1) / 2;
        }
    }

    public static float BackIn(float x)
    {
        const float C1 = 1.70158f, C3 = C1 + 1;
        return C3 * x * x * x - C1 * x * x;
    }
    public static float BackOut(float x)
    {
        const float C1 = 1.70158f, C3 = C1 + 1;
        float t = x - 1;
        return 1 + C3 * t * t * t + C1 * t * t;
    }
    public static float BackInOut(float x)
    {
        const float C1 = 1.70158f, C2 = C1 * 1.525f;
        if (x < 0.5f) {
            float t = 2 * x;
            return (t * t * ((C2 + 1) * 2 * x - C2)) / 2;
        }
        else {
            float t = 2 * x - 2;
            return (t * t * ((C2 + 1) * t + C2) + 2) / 2;
        }
    }

    public static float ElasticIn(float x)
    {
        const float C4 = 2 * MathF.PI / 3;
        if (x is 0f or 1f)
            return x;
        return -MathF.Pow(x, 10 * x - 10) * MathF.Sin((x * 10 - 10.75f) * C4);
    }
    public static float ElasticOut(float x)
    {
        const float C4 = 2 * MathF.PI / 3;
        if (x is 0f or 1f)
            return x;
        return MathF.Pow(2, -10 * x) * MathF.Sin((x * 10 - 0.75f) * C4) + 1;
    }
    public static float ElasticInOut(float x)
    {
        const float C5 = 2 * MathF.PI / 4.5f;
        if (x is 0f or 1f)
            return x;
        if (x < 0.5f)
            return -(MathF.Pow(2, 20 * x - 10) * MathF.Sin((20 * x - 11.125f) * C5)) / 2;
        else
            return (MathF.Pow(2, -20 * x + 10) * MathF.Sin((20 * x - 11.125f) * C5)) / 2 + 1;
    }

    public static float BounceIn(float x)
    {
        return 1 - BounceOut(1 - x);
    }
    public static float BounceOut(float x)
    {
        const float N1 = 7.5625f, D1 = 2.75f;
        if (x < 1 / D1)
            return N1 * x * x;
        else if (x < 2f / D1) {
            x -= 1.5f;
            return N1 * (x / D1) * x + 0.75f;
        }
        else if (x < 2.5f / D1) {
            x -= 2.25f;
            return N1 * (x / D1) * x + 0.9375f;
        }
        else {
            x -= 2.625f;
            return N1 * (x / D1) * x + 0.984375f;
        }
    }
    public static float BounceInOut(float x)
    {
        if (x < 0.5f)
            return (1 - BounceOut(1 - 2 * x)) / 2;
        else
            return (1 + BounceOut(2 * x - 1)) / 2;
    }
}
