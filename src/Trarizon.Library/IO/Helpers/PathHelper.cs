using Trarizon.Library.Helpers;

namespace Trarizon.Library.IO.Helpers;
internal static class PathHelper
{
    public static string Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
    {
        return StringHelper.Interpolated(stackalloc char[path1.Length + path2.Length + 1], null,
            $"{path1}{Path.DirectorySeparatorChar}{path2}").ToString();
    }

    public static string Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3)
    {
        return StringHelper.Interpolated(stackalloc char[path1.Length + path2.Length + path3.Length + 2], null,
            $"{path1}{Path.DirectorySeparatorChar}{path2}{Path.DirectorySeparatorChar}{path3}").ToString();
    }

    public static string Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, ReadOnlySpan<char> path4)
    {
        return StringHelper.Interpolated(stackalloc char[path1.Length + path2.Length + path3.Length + path4.Length + 3], null,
            $"{path1}{Path.DirectorySeparatorChar}{path2}{Path.DirectorySeparatorChar}{path3}{Path.DirectorySeparatorChar}{path4}").ToString();
    }
}
