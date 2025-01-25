using CommunityToolkit.HighPerformance;
using System.Diagnostics;
using Trarizon.Library.CodeGeneration;
using Trarizon.Library.Collections;
using Trarizon.Library.Numerics;

namespace Trarizon.Library;
public static partial class TraRandom
{
    /// <summary>
    /// Weight random
    /// </summary>
    /// <returns>The index of result in <paramref name="weights"/></returns>
    public static int SelectWeighted(this Random random, ReadOnlySpan<float> weights)
    {
        float totalWeight = 0;
        foreach (var w in weights) {
            totalWeight += w;
        }

        float value = random.NextSingle() * totalWeight;
        for (int i = 0; i < weights.Length; i++) {
            if (value < weights[i])
                return i;
            else
                value -= weights[i];
        }

        Debug.Assert(false, "Unreachable");
        return weights.Length - 1;
    }

    /// <summary>
    /// Weight random
    /// </summary>
    /// <returns>The index of result in <paramref name="weights"/></returns>
    public static int SelectWeighted(this Random random, List<float> weights)
        => random.SelectWeighted(weights.AsSpan());

    #region Next Value

    public static float NextSingle(this Random random, float min, float max)
#if NETSTANDARD2_0
        => TraNumber.Lerp(min, max, random.NextSingle());
#else
        => float.Lerp(min, max, random.NextSingle());
#endif

    public static double NextDouble(this Random random, double min, double max)
#if NETSTANDARD2_0
        => min + random.NextDouble() * (max - min);
#else
        => double.Lerp(min, max, random.NextDouble());
#endif

    public static bool NextBoolean(this Random random)
        => random.Next(2) != 0;

    public static T NextItem<T>(this Random random, ReadOnlySpan<T> items, [OptionalOut] out int index)
    {
        index = random.Next(items.Length);
        return items[index];
    }
    public static T NextItem<T>(this Random random, Span<T> items, [OptionalOut] out int index) => random.NextItem((ReadOnlySpan<T>)items, out index);
    public static T NextItem<T>(this Random random, T[] items, [OptionalOut] out int index) => random.NextItem(items.AsSpan(), out index);
    public static T NextItem<T>(this Random random, List<T> items, [OptionalOut] out int index) => random.NextItem(items.AsSpan(), out index);
    public static T NextItem<T>(this Random random, IReadOnlyList<T> items, [OptionalOut] out int index)
    {
        index = random.Next(items.Count);
        return items[index];
    }

    #endregion

#if NETSTANDARD2_0

    public static void Shuffle<T>(this Random random, Span<T> span)
    {
        for (int i = 0; i < span.Length - 1; i++) {
            int j = random.Next(i, span.Length);
            if (j != i) {
                (span[j], span[i]) = (span[i], span[j]);
            }
        }
    }

#endif
}
