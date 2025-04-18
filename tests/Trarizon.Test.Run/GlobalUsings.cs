global using static Trarizon.Test.Run.GlobalUsings;

using BenchmarkDotNet.Running;
using Microsoft.Diagnostics.Runtime;
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

    public static T[] ArrayValues<T>(Func<int, T> factory, int count = 8)
        => ArrayInts(count).Select(factory).ToArray();

    public static List<int> ListInts(int length = 8)
    {
        var res = new List<int>(length);
        CollectionsMarshal.SetCount(res, length);
        for (int i = 0; i < length; i++)
            res[i] = i;
        return res;
    }

    public static List<T> ListValues<T>(Func<int, T> factory, int count = 8)
        => ArrayInts(count).Select(factory).ToList();

    public static IEnumerable<int> EnumerateInts(int length = 8)
    {
        for (int i = 0; i < length; i++)
            yield return i;
    }

    public static IEnumerable<T> EnumerateCollection<T>(params IEnumerable<T> collection) => collection;

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
        PrintValue(Console.Write, value);
        Console.WriteLine();
    }

    public static void Print<T>(this ReadOnlySpan<T> values) => values.ToArray().Print();

    public static void PrintType<T>(this T value) => value?.GetType().Print();

    private static void PrintValue<T>(Action<string?> print, T value)
    {
        if (value is string strV) {
            print($@"""{strV}""");
            return;
        }

        if (value is IEnumerable collection) {
            PrintCollection(collection.Cast<object?>());
            return;
        }

        if (value is IEnumerable<object> geenumerable) {
            PrintCollection(geenumerable);
            return;
        }

        if (TryEnumerate(typeof(T), out var count) is { } enumerable) {
            PrintCollection(enumerable, count);
            return;
        }

        print(value?.ToString());

        IEnumerable<object?>? TryEnumerate(Type type, out int? count)
        {
            count = TryGetCount();

            var getEnumerator = type.GetMethod(nameof(IEnumerable.GetEnumerator));
            if (getEnumerator == null)
                return null;
            if (getEnumerator.IsStatic)
                return null;
            if (getEnumerator.GetParameters() is not [])
                return null;

            var enumerator = getEnumerator.Invoke(value, null);
            if (enumerator is null)
                return null;

            if (enumerator is IEnumerator en)
                return Enumerate(en);
            if (enumerator is IEnumerator<object> geen)
                return EnumerateG(geen);

            var enumeratorType = enumerator.GetType();
            var moveNext = enumeratorType.GetMethod(nameof(IEnumerator.MoveNext));
            if (moveNext == null)
                return null;
            if (moveNext.IsStatic)
                return null;
            if (moveNext.ReturnType != typeof(bool))
                return null;
            if (moveNext.GetParameters() is not [])
                return null;

            var current = enumeratorType.GetProperty(nameof(IEnumerator.Current));
            if (current == null)
                return null;
            if (current.PropertyType == null)
                return null;
            var get_current = current.GetMethod;
            if (get_current == null)
                return null;
            if (get_current.IsStatic)
                return null;

            return GetEnumerable();

            IEnumerable<object?> GetEnumerable()
            {
                var mn = moveNext.CreateDelegate<Func<bool>>(enumerator);
                var gc = get_current.CreateDelegate(typeof(Func<>).MakeGenericType(get_current.ReturnType!), enumerator);
                while (mn()) {
                    yield return gc.DynamicInvoke();
                }
            }

            IEnumerable<object?> Enumerate(IEnumerator enumerator)
            {
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }

            IEnumerable<object?> EnumerateG(IEnumerator<object> enumerator)
            {
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }

            int? TryGetCount()
            {
                var get_count = GetCount(nameof(Array.Length)) ?? GetCount(nameof(ICollection.Count));
                if (get_count == null)
                    return null;
                return get_count.Invoke(value, null)!.ForceCastTo<int>();

                MethodInfo? GetCount(string name)
                {
                    var prop = type.GetProperty(name);
                    if (prop == null)
                        return default;
                    if (prop.PropertyType != typeof(int))
                        return default;
                    var get_count = prop.GetMethod;
                    if (get_count == null)
                        return null;
                    if (get_count.IsStatic)
                        return null;
                    return get_count;
                }
            }
        }

        void PrintCollection(IEnumerable<object?> collection, int? count = null)
        {
            print("[");
            foreach (var (val, num) in collection.LookAhead(1)) {
                PrintValue(print, val);
                if (num > 0)
                    print(", ");
                else
                    print("]");
            }

            if (count is null) {
                if (collection is ICollection col)
                    count = col.Count;
                else if (collection is IReadOnlyCollection<object> roc)
                    count = roc.Count;
                else if (collection.GetType().GetInterfaces()
                    .Where(itf => itf.IsGenericType && itf.GetGenericTypeDefinition() == typeof(ICollection<>))
                    .Select(itf => itf.GenericTypeArguments[0])
                    .TryFirst(out var type)) {
                    count = typeof(ICollection<>).MakeGenericType(type)
                        .GetProperty(nameof(ICollection.Count))!
                        .GetValue(collection)!
                        .ForceCastTo<int>();
                }
            }

            if (count is { } c)
                print($"({c})");
        }
    }

    #endregion

    public static T ForceCastTo<T>(this object source) => (T)source;

    public static T With<T>(this T value, Func<T, T> selector) => selector(value);
}