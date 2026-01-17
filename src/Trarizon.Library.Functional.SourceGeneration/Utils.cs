using System.Collections.Generic;
using System.IO;

namespace Trarizon.Library.Functional.SourceGeneration;

internal static class Utils
{
    public static IEnumerable<T> JoinWriteEmptyLine<T>(this IEnumerable<T> source, TextWriter writer)
    {
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            yield break;
        var current = enumerator.Current;
        while (enumerator.MoveNext()) {
            yield return current;
            writer.WriteLine();
            current = enumerator.Current;
        }
        yield return current;
    }
}
