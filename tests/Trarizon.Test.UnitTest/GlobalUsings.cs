global using Microsoft.VisualStudio.TestTools.UnitTesting;
global using static Trarizon.Test.UnitTest.GlobalUsings;
global using static Trarizon.Test.Run.GlobalUsings;

namespace Trarizon.Test.UnitTest;

internal static class GlobalUsings
{
    extension(Assert)
    {
        public static void AreSequenceEqual<T>(IEnumerable<T> source, params T[] compare)
              => Assert.IsTrue(source.SequenceEqual(compare), $"[{string.Join(", ", source)}] != [{string.Join(", ", compare)}]");
    }

    public static void AssertSequenceEqual<T>(IEnumerable<T> source, params T[] compare)
        => Assert.IsTrue(source.SequenceEqual(compare), $"[{string.Join(", ", source)}] != [{string.Join(", ", compare)}]");
    public static void AssertSequenceEqual<T>(int id, IEnumerable<T> source, params T[] compare)
        => Assert.IsTrue(source.SequenceEqual(compare), $"{id}: [{string.Join(", ", source)}] != [{string.Join(", ", compare)}]");
}