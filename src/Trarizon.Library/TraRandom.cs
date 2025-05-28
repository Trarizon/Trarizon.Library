using System.Diagnostics;
using Trarizon.Library.CodeGeneration;
using Trarizon.Library.Collections;
using Trarizon.Library.Mathematics;
#if NETSTANDARD
using ListMarshal = Trarizon.Library.Collections.TraCollection;
using MathFLerp = Trarizon.Library.Mathematics.TraMath;
using MathDLerp = Trarizon.Library.Mathematics.TraMath;
#else
using ListMarshal = System.Runtime.InteropServices.CollectionsMarshal;
using MathFLerp = float;
using MathDLerp = double;
#endif


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
        => random.SelectWeighted(ListMarshal.AsSpan(weights));

    #region Next Value

    public static float NextSingle(this Random random, float min, float max)
        => MathFLerp.Lerp(min, max, random.NextSingle());

    public static double NextDouble(this Random random, double min, double max)
        => MathDLerp.Lerp(min, max, random.NextDouble());

    public static bool NextBoolean(this Random random)
        => random.Next(2) != 0;

    public static T NextItem<T>(this Random random, ReadOnlySpan<T> items, [OptionalOut] out int index)
    {
        index = random.Next(items.Length);
        return items[index];
    }

    #endregion
}
