global using static GlobalUsings;

using BenchmarkDotNet.Running;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using Trarizon.Test.Run;

internal static class GlobalUsings
{
    public static void RunBenchmarks() => BenchmarkRunner.Run<Benchmarks>();

    public static int[] ArrayInts(int length = 8)
    {
        var res = new int[length];
        for (int i = 0; i < length; i++)
            res[i] = i;
        return res;
    }

    public static List<int> ListInts(int length = 8)
    {
        var res = new List<int>(length);
        CollectionsMarshal.SetCount(res, length);
        for (int i = 0; i < length; i++)
            res[i] = i;
        return res;
    }

    public static IEnumerable<int> EnumerateInts(int length = 8)
    {
        for (int i = 0; i < length; i++)
            yield return i;
    }

    #region Print

    public static void Print(this string? value)
        => Console.WriteLine(value ?? "<null>");
    public static void Print<T>(this T value) => Print(PrintValue(value));
    public static void Print<T>(this IEnumerable<T> values) => Print(PrintValue(values));
    public static void Print<T>(this Span<T> values) => Print(PrintValue((ReadOnlySpan<T>)values));
    public static void Print<T>(this ReadOnlySpan<T> values) => Print(PrintValue(values));

    private static string? PrintValue<T>(T value)
    {
        switch (value) {
            case IEnumerable collection:
                var sb = new StringBuilder();
                foreach (var val in collection) {
                    sb.Append(PrintValue(val)).Append(", ");
                }
                if (sb.Length > 0)
                    sb.Length -= 2;
                return $"[{sb}]";
            default:
                return value?.ToString();
        }
    }
    private static string PrintValue<T>(ReadOnlySpan<T> values)
    {
        var sb = new StringBuilder();
        sb.Append('[');
        foreach (var val in values) {
            sb.Append(PrintValue(val)).Append(", ");
        }
        sb.Length--;
        sb[^1] = ']';
        return sb.ToString();
    }

    #endregion

    public static T ForceCastTo<T>(this object source) => (T)source;
}
