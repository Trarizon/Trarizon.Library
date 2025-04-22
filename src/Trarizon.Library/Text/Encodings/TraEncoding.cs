using System.Text;

namespace Trarizon.Library.Text.Encodings;
public static class TraEncoding
{
#if NETSTANDARD
    public static unsafe int GetByteCount(this Encoding encoding, ReadOnlySpan<char> chars)
    {
        if (chars.IsEmpty)
            return 0;
        fixed (char* ch = chars) {
            return encoding.GetByteCount(ch, chars.Length);
        }
    }

    public static unsafe int GetBytess(this Encoding encoding, ReadOnlySpan<char> chars, Span<byte> bytes)
    {
        if (chars.IsEmpty)
            return 0;
        fixed (char* ch = chars) fixed (byte* by = bytes) {
            return encoding.GetBytes(ch, chars.Length, by, bytes.Length);
        }
    }

    public static unsafe int GetCharCount(this Encoding encoding, ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
            return 0;
        fixed (byte* by = bytes) {
            return encoding.GetCharCount(by, bytes.Length);
        }
    }

    public static unsafe int GetChars(this Encoding encoding, ReadOnlySpan<byte> bytes, Span<char> chars)
    {
        if (bytes.IsEmpty)
            return 0;
        fixed (byte* by = bytes) {
            return encoding.GetChars(bytes, chars);
        }
    }

    public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
            return string.Empty;
        fixed (byte* by = bytes) {
            return encoding.GetString(by, bytes.Length);
        }
    }

#endif
}
