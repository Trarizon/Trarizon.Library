global using Microsoft.VisualStudio.TestTools.UnitTesting;
global using static GlobalUsings;

internal static class GlobalUsings
{
    public static void AssertSequenceEqual<T>(IEnumerable<T> source, params T[] compare)
        => Assert.IsTrue(source.SequenceEqual(compare), $"[{string.Join(", ", source)}] != [{string.Join(", ", compare)}]");
    public static void AssertSequenceEqual<T>(int id, IEnumerable<T> source, params T[] compare)
        => Assert.IsTrue(source.SequenceEqual(compare), $"{id}: [{string.Join(", ", source)}] != [{string.Join(", ", compare)}]");

    public static int[] ArrayInts(int count = 8)
        => new int[count].Select((_, i) => i).ToArray();

    public static T[] ArrayValues<T>(Func<int, T> factory, int count = 8)
        => ArrayInts(count).Select(factory).ToArray();

    public static IEnumerable<int> EnumerateInts(int count = 8)
    {
        for (int i = 0; i < count; i++) yield return i;
    }

    public static IEnumerable<T> EnumerateValues<T>(Func<int, T> factory, int count = 8)
        => EnumerateInts(count).Select(factory);
}