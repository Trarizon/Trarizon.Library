using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Text.Json.Internal;
internal static class Throws
{
    [DoesNotReturn]
    public static void ThrowNotSupport(string? message = null) => throw new NotSupportedException(message);
}
