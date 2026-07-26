using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library;

#if NETSTANDARD

internal static partial class Polyfills
{
    public static float NextSingle(this Random random)
        => (float)random.NextDouble();

    public static long NextInt64(this Random random)
    {
#if NETSTANDARD2_0
        long low, high;
        low = random.Next();
        high = random.Next();
        return (high << 32) | low;
#else
        var bytes = (stackalloc byte[sizeof(long)]);
        random.NextBytes(bytes);
        return Unsafe.ReadUnaligned<long>(ref MemoryMarshal.GetReference(bytes));
#endif
    }

    public static void Shuffle<T>(this Random random, Span<T> span)
    {
        for (int i = 0; i < span.Length - 1; i++)
        {
            int j = random.Next(i, span.Length);
            if (j != i)
            {
                (span[j], span[i]) = (span[i], span[j]);
            }
        }
    }
}

#endif
