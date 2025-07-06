using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Text;
public static partial class TraString
{
    private const int MaxStackAllocCharLength = 256;

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

    public static string Unescape(ReadOnlySpan<char> chars, bool noThrowUnknownEscape = false)
    {
        if (chars.Length == 0)
            return "";

        if (chars.Length <= MaxStackAllocCharLength) {
            var buffer = (stackalloc char[chars.Length]);
            if (TryUnescape(chars, buffer, out var written, noThrowUnknownEscape)) {
                return buffer[..written].ToString();
            }
            else {
                Throws.ThrowInvalidOperation("Invalid escaped sequence");
                return default;
            }
        }
        else {
            char[] rented = ArrayPool<char>.Shared.Rent(chars.Length);
            try {
                if (TryUnescape(chars, rented, out var written, noThrowUnknownEscape)) {
                    return rented.AsSpan()[..written].ToString();
                }
                else {
                    Throws.ThrowInvalidOperation("Invalid escaped sequence");
                    return default;
                }
            }
            finally {
                ArrayPool<char>.Shared.Return(rented);
            }
        }
    }

    /// <param name="buffer">
    /// Where unescaped characters written, exception thrown if buffer to short. 
    /// To avoid error, make sure buffer.Length is greater than or equals to chars.Length
    /// </param>
    /// <param name="writtenCount">Written count won't be greater than chars.Length</param>
    /// <param name="reserveUnknownEscapeChar">If <see langword="true"/>, the method won't return false</param>
    public static bool TryUnescape(ReadOnlySpan<char> chars, Span<char> buffer, out int writtenCount, bool reserveUnknownEscapeChar = false)
    {
        int bfrIdx = 0;

        int nonEscapeStart = 0;
        int i = 0;
        while (i < chars.Length) {
            if (chars[i] is not '\\') {
                goto Continue;
            }

            if (i + 1 == chars.Length) {
                if (!reserveUnknownEscapeChar)
                    goto Error;
                goto Continue;
            }

            var raw = chars[nonEscapeStart..i];
            raw.CopyTo(buffer[bfrIdx..]);
            bfrIdx += raw.Length;

            int backSlashIndex = i;
            i++;
            var ch = chars[i];

            switch (ch) {
                case '\\' or '\'' or '\"': buffer[bfrIdx++] = ch; break;
                case '0': buffer[bfrIdx++] = '\0'; break;
                case 'a': buffer[bfrIdx++] = '\a'; break;
                case 'b': buffer[bfrIdx++] = '\b'; break;
                case 'e': buffer[bfrIdx++] = '\e'; break;
                case 'f': buffer[bfrIdx++] = '\f'; break;
                case 'n': buffer[bfrIdx++] = '\n'; break;
                case 'r': buffer[bfrIdx++] = '\r'; break;
                case 't': buffer[bfrIdx++] = '\t'; break;
                case 'v': buffer[bfrIdx++] = '\v'; break;
                case 'u': { // unicode \uXXXX
                    if (i + 4 < chars.Length) {
                        int hex = 0;
                        for (int k = 1; k < 5; k++) {
                            var num = GetHex(chars[i + k]);
                            if (num >= 0)
                                hex = (hex << 4) + num;
                            else
                                goto UnicodeFail;
                        }
                        buffer[bfrIdx++] = (char)hex;
                        i += 4;
                        break;
                    }
                UnicodeFail:
                    if (!reserveUnknownEscapeChar)
                        goto Error;
                    goto UnknownEscapeContinue;
                }
                case 'x': { // \xX ~ \xXXX
                    if (i + 1 < chars.Length) {
                        int hex = 0;
                        int k;
                        for (k = 1; k < 5; k++) {
                            var num = GetHex(chars[i + k]);
                            if (num >= 0)
                                hex = (hex << 4) + num;
                            else
                                break; // for
                        }
                        if (k > 1) {
                            buffer[bfrIdx++] = (char)hex;
                            i += k - 1;
                            break; // switch
                        }
                    }

                    if (!reserveUnknownEscapeChar)
                        goto Error;
                    goto UnknownEscapeContinue;
                }
                default:
                    goto Error;
            }

            i++;
            nonEscapeStart = i;
            continue;

        UnknownEscapeContinue:
            nonEscapeStart = backSlashIndex;
        Continue:
            i++;
        }
        var rest = chars[nonEscapeStart..];
        rest.CopyTo(buffer[bfrIdx..]);
        bfrIdx += rest.Length;
        writtenCount = bfrIdx;
        return true;

    Error:
        writtenCount = default;
        return false;

        static int GetHex(char ch)
        {
            if (ch is >= '0' and <= '9')
                return ch - '0';
            if (ch is >= 'a' and <= 'f')
                return ch - ('a' - 10);
            if (ch is >= 'A' and <= 'F')
                return ch - ('A' - 10);
            return -1;
        }
    }
}
