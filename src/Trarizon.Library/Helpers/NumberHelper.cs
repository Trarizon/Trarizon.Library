using System.Numerics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Helpers;
public static partial class NumberHelper
{
    #region Float.Remap

    /// <summary>
    /// Map a floating number from [0,1] into another range
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TFloat Remap<TFloat>(this TFloat number, TFloat lowerBound, TFloat intervalLength) where TFloat : struct, IBinaryFloatingPointIeee754<TFloat>
        => number * intervalLength + lowerBound;

    /// <summary>
    /// Map a floating number from a range into another range
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TFloat Remap<TFloat>(this TFloat number, TFloat oriLowerBound, TFloat oriIntervalLength, TFloat lowerBound, TFloat intervalLength) where TFloat : struct, IBinaryFloatingPointIeee754<TFloat>
        => Remap((number - oriLowerBound) / oriIntervalLength, lowerBound, intervalLength);

    /// <summary>
    /// Map a floating number from [0,1] into another range
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TFloat RemapInto<TFloat>(this TFloat number, TFloat minBoundary, TFloat maxBoundary) where TFloat : struct, IBinaryFloatingPointIeee754<TFloat>
        => Remap(number, minBoundary, maxBoundary - minBoundary);

    /// <summary>
    /// Map a floating number from a range into another range
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TFloat RemapInto<TFloat>(this TFloat number, TFloat originalMinBoundary, TFloat originalMaxBoundary, TFloat minBoundary, TFloat maxBoundary) where TFloat : struct, IBinaryFloatingPointIeee754<TFloat>
        => Remap(number, originalMinBoundary, originalMaxBoundary - originalMinBoundary, minBoundary, maxBoundary - minBoundary);

    #endregion

    internal static int GetUnderlyingInt32(this Index index) => Unsafe.As<Index, int>(ref index);
}
