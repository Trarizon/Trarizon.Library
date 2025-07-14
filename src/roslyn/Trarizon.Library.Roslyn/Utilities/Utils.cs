using System.Runtime.CompilerServices;

namespace Trarizon.Library.Roslyn.Utilities;
internal static class Utils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint RoundUpToPowerOf2(uint value)
    {
        --value;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        return value + 1;
    }
}
