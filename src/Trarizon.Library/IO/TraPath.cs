using System.Buffers;

namespace Trarizon.Library.IO;
public static class TraPath
{
    private static readonly SearchValues<char> _invalidFileNameChars = SearchValues.Create(Path.GetInvalidFileNameChars());
   
    public static bool ContainsInvalidFileNameChar(ReadOnlySpan<char> fileName)
    {
        foreach (var c in fileName) {
            if (_invalidFileNameChars.Contains(c))
                return true;
        }
        return false;
    }

    public static void ReplaceInvalidFileChars(Span<char> input, char newChar)
    {
        foreach (ref var c in input) {
            if (_invalidFileNameChars.Contains(c))
                c = newChar;
        }
    }

    public static string ReplaceInvalidFileChars(string fileName, char newChar)
    {
        Span<char> buffer = stackalloc char[fileName.Length];
        int lastReplaceIndex = -1;

        for (int i = 0; i < fileName.Length; i++) {
            if (_invalidFileNameChars.Contains(fileName[i])) {
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
