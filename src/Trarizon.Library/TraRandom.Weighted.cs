using System.Diagnostics;

namespace Trarizon.Library;

public static partial class TraRandom
{
    /// <returns>The index of result in <paramref name="weights"/></returns>
    public static int Next(this Random random, ReadOnlySpan<int> weights)
    {
        int totalWeight = 0;
        foreach (var w in weights)
            totalWeight += w;

        int value = random.Next(totalWeight);
        for (int i = 0; i < weights.Length; i++)
        {
            if (value < weights[i])
                return i;

            value -= weights[i];
        }

        Debug.Assert(false, "Unreachable");
        return weights.Length - 1;
    }

    /// <returns>The index of result in <paramref name="weights"/></returns>
    public static int Next(this Random random, ReadOnlySpan<long> weights)
    {
        long totalWeight = 0;
        foreach (var w in weights)
            totalWeight += w;

        long value = random.NextInt64(totalWeight);
        for (int i = 0; i < weights.Length; i++)
        {
            if (value < weights[i])
                return i;

            value -= weights[i];
        }

        Debug.Assert(false, "Unreachable");
        return weights.Length - 1;
    }

    /// <returns>The index of result in <paramref name="weights"/></returns> 
    public static int Next(this Random random, ReadOnlySpan<float> weights)
    {
        float totalWeight = 0;
        foreach (var w in weights)
            totalWeight += w;

        float value = random.NextSingle() * totalWeight;
        for (int i = 0; i < weights.Length; i++)
        {
            if (value < weights[i])
                return i;

            value -= weights[i];
        }

        Debug.Assert(false, "Unreachable");
        return weights.Length - 1;
    }

    /// <returns>The index of result in <paramref name="weights"/></returns>
    public static int Next(this Random random, ReadOnlySpan<double> weights)
    {
        double totalWeight = 0;
        foreach (var w in weights)
            totalWeight += w;

        double value = random.NextDouble() * totalWeight;
        for (int i = 0; i < weights.Length; i++)
        {
            if (value < weights[i])
                return i;

            value -= weights[i];
        }

        Debug.Assert(false, "Unreachable");
        return weights.Length - 1;
    }
}
