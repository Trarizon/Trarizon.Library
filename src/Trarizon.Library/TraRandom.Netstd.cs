#if NETSTANDARD2_0

namespace Trarizon.Library;
partial class TraRandom
{
    public static float NextSingle(this Random rand)
        => (float)rand.NextDouble();
}

#endif
