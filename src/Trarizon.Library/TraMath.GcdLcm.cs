using System.Numerics;

namespace Trarizon.Library;

public static partial class TraMath
{
#if NET7_0_OR_GREATER

    extension<T>(T) where T : IBinaryInteger<T>
    {
        public static T Gcd(T left, T right)
        {
            while (right != T.Zero)
                (left, right) = (right, left % right);
            return left;
        }

        public static T Lcm(T left, T right)
            => left * right / Gcd(left, right);
    }

#else

    extension(int)
    {
        public static int Gcd(int left, int right)
        {
            while (right != 0)
                (left, right) = (right, left % right);
            return left;
        }

        public static int Lcm(int left, int right)
            => left * right / Gcd(left, right);
    }

    extension(long)
    {
        public static long Gcd(long left, long right)
        {
            while (right != 0)
                (left, right) = (right, left % right);
            return left;
        }

        public static long Lcm(long left, long right)
            => left * right / Gcd(left, right);
    }

#endif
}