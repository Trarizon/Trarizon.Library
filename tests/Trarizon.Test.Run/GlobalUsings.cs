global using static Trarizon.Test.Run.GlobalUsings;

using BenchmarkDotNet.Running;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Trarizon.Library.Collections;

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

    public static void Print<T>(this T value)
    {
        Console.WriteLine(PrintValue(value));
    }

    public static void Print<T>(this Span<T> values) => PrintValue((ReadOnlySpan<T>)values).Print();
    public static void Print<T>(this ReadOnlySpan<T> values) => PrintValue(values).Print();

    public static void PrintType<T>(this T value) => value?.GetType().Print();

    private static string? PrintValue<T>(T value)
    {
        if (value is string strV)
            return $@"""{strV}""";

        if (value is IEnumerable collection) {
            return GetCollectionString(collection);
        }

        if (TryEnumerate(typeof(T)).TryToNonEmptyList(out var list)) {
            return GetCollectionString(list);
        }

        return value?.ToString();

        IEnumerable<object?> TryEnumerate(Type type)
        {
            var getEnumerator = type.GetMethod(nameof(IEnumerable.GetEnumerator))!;
            if (getEnumerator == null)
                yield break;
            if (getEnumerator.IsStatic)
                yield break;
            if (getEnumerator.GetParameters() is not [])
                yield break;

            var enumeratorType = getEnumerator.ReturnType;
            var moveNext = enumeratorType.GetMethod(nameof(IEnumerator.MoveNext))!;
            if (moveNext == null)
                yield break;
            if (moveNext.IsStatic)
                yield break;
            if (moveNext.GetParameters() is not [])
                yield break;
            if (moveNext.ReturnType != typeof(bool))
                yield break;

            var current = enumeratorType.GetProperty(nameof(IEnumerator.Current));
            if (current == null)
                yield break;
            if (current.PropertyType == null)
                yield break;
            var get_current = current.GetMethod!;
            if (get_current == null)
                yield break;

            var enumerator = getEnumerator.Invoke(value, null);
            var mn = moveNext.CreateDelegate<Func<bool>>(enumerator);
            var gc = get_current.CreateDelegate(typeof(Func<>).MakeGenericType(get_current.ReturnType!), enumerator);
            while (mn()) {
                yield return gc.DynamicInvoke();
            }
        }

        static string GetCollectionString(IEnumerable collection)
        {
            var sb = new StringBuilder();
            foreach (var val in collection) {
                sb.Append(PrintValue(val)).Append(", ");
            }
            if (sb.Length > 0)
                sb.Length -= 2;

            int? count = null;
            if (collection is ICollection col)
                count = col.Count;
            else if (collection is IReadOnlyCollection<int> roc)
                count = roc.Count;
            else if (typeof(T).GetInterfaces()
                .Where(itf => itf.IsGenericType && itf.GetGenericTypeDefinition() == typeof(ICollection<>))
                .Select(itf => itf.GenericTypeArguments[0])
                .TryFirst(out var type)) {
                count = (int)typeof(ICollection<>).MakeGenericType(type)
                    .GetProperty(nameof(ICollection<T>.Count))!
                    .GetValue(collection)!;
            }

            if (count == null) return $"[{sb}]";
            else return $"[{sb}]({count})";
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

    public static T With<T>(this T value, Func<T, T> selector) => selector(value);
}