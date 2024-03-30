using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Helpers;
public static class StringHelper
{
    [SuppressMessage("Style", "IDE0060", Justification = "For interpolated string handler")]
    public static string Interpolated(
        Span<char> initialBuffer,
        IFormatProvider? provider,
        [InterpolatedStringHandlerArgument(nameof(provider), nameof(initialBuffer))]
        in DefaultInterpolatedStringHandler handler)
        => handler.ToString();
}
