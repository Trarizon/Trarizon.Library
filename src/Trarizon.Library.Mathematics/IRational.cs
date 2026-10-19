using System.Numerics;

namespace Trarizon.Library.Mathematics;

public interface IRational<TSelf, TNumber>
    where TSelf : IRational<TSelf, TNumber>
    where TNumber : IBinaryInteger<TNumber>
{
    TNumber Numerator { get; }
    TNumber Denominator { get; }
}