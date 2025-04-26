using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library;
[StackTraceHidden]
internal static class TraThrow
{
    public const string ExpNoUse = "NoUse";

    [DoesNotReturn]
    internal static void Exception(Exception exception)
        => throw exception;

    [DoesNotReturn]
    public static T InvalidEnumState<T>(Enum state)
        => throw new InvalidOperationException($"Invalid enum state: {state}");
}
