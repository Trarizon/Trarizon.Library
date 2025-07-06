using System.Runtime.CompilerServices;

namespace Trarizon.Library.Linq.Helpers;
internal static partial class Throws
{
    public static void IfNegative(int value, [CallerArgumentExpression(nameof(value))] string name = "")
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNegative(value, name);
#else
        if (value < 0)
            ThrowArgumentOutOfRange(name, value, "Value must be a non-negatie value.");
#endif
    }

    public static void IfGreaterThan(int value, int other, [CallerArgumentExpression(nameof(value))] string name = "")
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other, name);
#else
        if (value > other)
            ThrowArgumentOutOfRange(name, value, $"Value must be less than or equal to '{other}'.");
#endif
    }

    public static void IfLessThan(int value, int other, [CallerArgumentExpression(nameof(value))] string name = "")
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfLessThan(value, other, name);
#else
        if (value < other)
            ThrowArgumentOutOfRange(name, value, $"Value must be greater than or equal to '{other}'.");
#endif
    }

    public static void IfNegativeOrGreaterThanOrEqual(int value, int other, [CallerArgumentExpression(nameof(value))] string name = "")
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)value, (uint)other, name);
#else
        if ((uint)value >= (uint)other)
            ThrowArgumentOutOfRange(name, value, $"Value must be positive and less than '{other}'.");
#endif
    }
}
