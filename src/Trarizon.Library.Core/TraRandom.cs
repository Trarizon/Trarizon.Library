namespace Trarizon.Library;

public static partial class TraRandom
{
    public static float NextSingle(this Random random, float max)
        => random.NextSingle() * max;

    public static float NextSingle(this Random random, float min, float max)
        => float.Lerp(min, max, random.NextSingle());

    public static double NextDouble(this Random random, double min, double max)
        => double.Lerp(min, max, random.NextDouble());

    public static bool NextBoolean(this Random random)
        => random.Next(2) != 0;
}
