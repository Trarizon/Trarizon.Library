using System.Numerics;

namespace Trarizon.Library.Mathematics;

public static partial class Interval
{
    extension<T>(Interval<T>) where T : IFloatingPointIeee754<T>
    {
        public static Interval<T> Unbounded() => Interval<T>.Open(T.NegativeInfinity, T.PositiveInfinity);
    }
}
