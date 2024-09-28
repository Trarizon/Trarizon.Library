using System.Buffers;

namespace Trarizon.Library.IO;
public static class TraPath
{
    private static readonly SearchValues<char> _invalidFileNameCharsSV = SearchValues.Create(Path.GetInvalidFileNameChars());

    public static bool ContainsInvalidFileNameChar(ReadOnlySpan<char> fileName)
    {
        foreach (var c in fileName) {
            if (_invalidFileNameCharsSV.Contains(c))
                return true;
        }
        return false;
    }

    public static void ReplaceInvalidFileChars(Span<char> input, char newChar)
    {
        foreach (ref var c in input) {
            if (_invalidFileNameCharsSV.Contains(c))
                c = newChar;
        }
    }

    public static string ReplaceInvalidFileChars(string fileName, char newChar)
    {
        Span<char> buffer = stackalloc char[fileName.Length];
        int lastReplaceIndex = -1;

        for (int i = 0; i < fileName.Length; i++) {
            if (_invalidFileNameCharsSV.Contains(fileName[i])) {
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

    public static string Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
    {
        return TraString.Interpolated(stackalloc char[path1.Length + path2.Length + 1], null,
            $"{path1}{Path.DirectorySeparatorChar}{path2}").ToString();
    }

    public static string Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3)
    {
        return TraString.Interpolated(stackalloc char[path1.Length + path2.Length + path3.Length + 2], null,
            $"{path1}{Path.DirectorySeparatorChar}{path2}{Path.DirectorySeparatorChar}{path3}").ToString();
    }

    public static string Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, ReadOnlySpan<char> path4)
    {
        return TraString.Interpolated(stackalloc char[path1.Length + path2.Length + path3.Length + path4.Length + 3], null,
            $"{path1}{Path.DirectorySeparatorChar}{path2}{Path.DirectorySeparatorChar}{path3}{Path.DirectorySeparatorChar}{path4}").ToString();
    }
}
