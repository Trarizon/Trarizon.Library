global using static Trarizon.Test.Run.GlobalUsings;

using BenchmarkDotNet.Running;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Trarizon.Test.Run;
public static class GlobalUsings
{
    public static void RunBenchmarks() => BenchmarkRunner.Run<Benchmarks>();

    #region Collections

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

    public static T[] ArrayValues<T>(Func<int, T> factory, int count = 8)
        => ArrayInts(count).Select(factory).ToArray();

    public static List<T> ListValues<T>(Func<int, T> factory, int count = 8)
        => ArrayInts(count).Select(factory).ToList();

    public static IEnumerable<T> EnumerateValues<T>(Func<int, T> factory, int count = 8)
        => EnumerateInts(count).Select(factory);

    public static IEnumerable<int> EnumerateLogged(int length = 8, TextWriter? printer = null)
    {
        printer ??= Console.Out;
        for (int i = 0; i < length; i++) {
            printer.WriteLine($"Enumerate {i}");
            yield return i;
        }
    }

    #endregion

    #region Print

    public static void Print(this string? value) => Console.WriteLine(value ?? "<null>");
    public static void Print<T>(this T value) => PrintValue(value).Print();
    public static void Print<T>(this Span<T> values) => PrintValue((ReadOnlySpan<T>)values).Print();
    public static void Print<T>(this ReadOnlySpan<T> values) => PrintValue(values).Print();

    private static string? PrintValue<T>(T value)
    {
        switch (value) {
            case IEnumerable collection and not string:
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