using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Text;
public static partial class TraString
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// Create string by <see cref="DefaultInterpolatedStringHandler"/>, directly return inner ReadOnlySpan without allocate string
    /// </summary>
    [SuppressMessage("Style", "IDE0060", Justification = "InterpoaltedStringArguments")]
    public static ReadOnlySpan<char> CreateAsSpan(IFormatProvider? formatProvider, Span<char> initalBuffer,
        [InterpolatedStringHandlerArgument(nameof(formatProvider), nameof(initalBuffer))] ref DefaultInterpolatedStringHandler handler)
        => Utils.GetTextSpan(ref handler);

    private static class Utils
    {
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Text")]
        public static extern ReadOnlySpan<char> GetTextSpan(ref DefaultInterpolatedStringHandler handler);
    }
#endif
}
