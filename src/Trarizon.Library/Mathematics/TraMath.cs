using System.Numerics;

namespace Trarizon.Library.Mathematics;
public static partial class TraMath
{
#if NET7_0_OR_GREATER
    public static T GreatestCommonDivisor<T>(T left, T right) where T : IBinaryInteger<T>
    {
        while (right != T.Zero) {
            var tmp = left;
            left = right;
            right = tmp % right;
        }
        return left;
    }

    public static T LeastCommonMultiple<T>(T left, T right) where T : IBinaryInteger<T> 
        => left * right / GreatestCommonDivisor(left, right);

#endif

    public static int GreatestCommonDivisor(int left, int right)
    {
        while (right != 0) {
            var tmp = left;
            left = right;
            right = tmp % right;
        }
        return left;
    }

    public static int LeastCommonMultiple(int left, int right) 
        => left * right / GreatestCommonDivisor(left, right);

}
