using System.Runtime.CompilerServices;

namespace Trarizon.Test.UnitTests;

internal static class Extensions
{
    public static T Var<T>(this T source, out T result) => result = source;

    public static IEnumerable<T> Enumerate<T>(this IEnumerable<T> source)
    {
        foreach (var item in source)
            yield return item;
    }

    extension(Assert)
    {
        public static unsafe void RefEqual<T>(in T expected, in T actual)
        {
            if (Unsafe.AreSame(in expected, in actual))
                return;
            Assert.Fail($"Expected and actual are not the same reference. expected: {(nint)Unsafe.AsPointer(in expected)}, actual: {(nint)Unsafe.AsPointer(in actual)}");
        }
    }
}
