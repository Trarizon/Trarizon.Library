#if NETSTANDARD

namespace Trarizon.Library;
public static partial class TraRandom
{
    public static float NextSingle(this Random rand)
        => (float)rand.NextDouble();
}

#endif
