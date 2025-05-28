
namespace Trarizon.Library;
public static partial class TraRandom
{
#if NETSTANDARD
    public static float NextSingle(this Random rand)
        => (float)rand.NextDouble();

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

