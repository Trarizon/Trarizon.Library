using System.Buffers;
using Trarizon.Library.CodeAnalysis.MemberAccess;

namespace Trarizon.Library.IO;
public static class TraPath
{
    [BackingFieldAccess(nameof(InvalidFileNameChars))]
    private static SearchValues<char>? _invalidFileNameChars;
    private static SearchValues<char> InvalidFileNameChars => _invalidFileNameChars ??= SearchValues.Create(Path.GetInvalidFileNameChars());
 
    [BackingFieldAccess(nameof(InvalidPathNameChars))]
    private static SearchValues<char>? _invalidPathNameChars;
    private static SearchValues<char> InvalidPathNameChars => _invalidPathNameChars ??= SearchValues.Create(Path.GetInvalidPathChars());

    public static bool IsValidFileName(ReadOnlySpan<char> fileName)
    {
        var chars = InvalidFileNameChars;
        foreach (var c in fileName) {
            if (chars.Contains(c))
                return false;
        }
        return true;
    }

    public static bool IsValidPathName(ReadOnlySpan<char> fileName)
    {
        var chars = InvalidPathNameChars;
        foreach (var c in fileName) {
            if (chars.Contains(c))
                return false;
        }
        return true;
    }

    public static void ReplaceInvalidFileNameChars(Span<char> input, char newChar)
    {
        var chars = InvalidFileNameChars;
        foreach (ref var c in input) {
            if (chars.Contains(c))
                c = newChar;
        }
    }

    public static string ReplaceInvalidFileNameChars(string fileName, char newChar)
    {
        Span<char> buffer = stackalloc char[fileName.Length];
        int lastReplaceIndex = -1;

        var chars = InvalidFileNameChars;
        for (int i = 0; i < fileName.Length; i++) {
            if (chars.Contains(fileName[i])) {
                CopyToBuffer(buffer, i);
                buffer[i] = newChar;
                lastReplaceIndex = i;
            }
        }

        if (lastReplaceIndex == -1)
            return fileName;

        CopyToBuffer(buffer, fileName.Length);
        return buffer.ToString();

        void CopyToBuffer(Span<char> buffer, int end)
        {
            var start = lastReplaceIndex + 1;
            fileName.AsSpan(start..end).CopyTo(buffer[start..end]);
        }
    }
}
