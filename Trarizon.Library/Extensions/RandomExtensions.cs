﻿using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Extensions;
public static partial class RandomExtensions
{
    #region SelectWeighted

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

        Debug.Assert(false);
        return weights.Length - 1;
    }

    /// <summary>
    /// Weight random
    /// </summary>
    /// <returns>The index of result in <paramref name="weights"/></returns>
    public static int SelectWeighted(this Random random, List<float> weights)
        => random.SelectWeighted(CollectionsMarshal.AsSpan(weights));

    #endregion

    #region Shuffle

    public static void Shuffle<T>(this Random random, List<T> list)
        => random.Shuffle(CollectionsMarshal.AsSpan(list));

    #endregion

    #region NextFloat

    public static float NextSingle(this Random random, float min, float max)
        => random.NextSingle() * (max - min) + min;

    public static double NextDouble(this Random random, double min, double max)
        => random.NextDouble() * (max - min) + min;

    #endregion
}
