global using Microsoft.VisualStudio.TestTools.UnitTesting;
global using static GlobalUsings;

internal static class GlobalUsings
{
    public static void AssertSequenceEqual<T>(IEnumerable<T> source, params T[] compare)
        => Assert.IsTrue(source.SequenceEqual(compare), $"[{string.Join(", ", source)}] != [{string.Join(", ", compare)}]");
    public static void AssertSequenceEqual<T>(int id, IEnumerable<T> source, params T[] compare)
        => Assert.IsTrue(source.SequenceEqual(compare), $"{id}: [{string.Join(", ", source)}] != [{string.Join(", ", compare)}]");

    public static int[] Array(int count = 8)
    {
        int[] array = new int[count];
        for (int i = 0; i < count; i++) array[i] = i;
        return array;
    }

    public static IEnumerable<int> Enumerate(int count = 8)
    {
        for (int i = 0; i < count; i++)
            yield return i;
    }
}