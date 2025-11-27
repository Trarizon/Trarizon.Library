using FluentAssertions;
using Trarizon.Library.Algorithms;
using Trarizon.Library.Collections;

namespace Trarizon.Test.UnitTests.Playground.Algorithms;

public class CollectionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void QSortTest(int seed)
    {
        var a = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        new Random(seed).Shuffle(a);
        a.Do(a => CollectionAlgorithm.QSort(a))
            .Should().Equal(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        a.Do(a => CollectionAlgorithm.QSort(a, Comparer<int>.Default.Reverse()))
            .Should().Equal(10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void NthLargestTest(int seed)
    {
        var a = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        new Random(seed).Shuffle(a);
        Enumerable.Range(0, 11)
            .Select(i => CollectionAlgorithm.NthSmallest(a.ToArray(), i))
            .Should().Equal(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        Enumerable.Range(0, 11)
            .Select(i => CollectionAlgorithm.NthSmallest(a.ToArray(), i))
            .Should().Equal(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
    }
}
