using FluentAssertions;
using Trarizon.Library.Linq;

namespace Trarizon.Test.UnitTests.Linq;

public class EnumerableTests
{
    [Theory]
    [MemberData(nameof(IntDatas))]
    public void AdjacentTest(IEnumerable<int> source)
    {
        source.Adjacent().Should().Equal((0, 1), (1, 2), (2, 3), (3, 4), (4, 5), (5, 6), (6, 7));
    }

    [Theory]
    [MemberData(nameof(IntDatas2), 3, 5)]
    public void CartesianProductTest(IEnumerable<int> first, IEnumerable<int> second)
    {
        first.CartesianProduct(second).Should().Equal(
            (0, 10), (0, 11), (0, 12), (0, 13), (0, 14),
            (1, 10), (1, 11), (1, 12), (1, 13), (1, 14),
            (2, 10), (2, 11), (2, 12), (2, 13), (2, 14));
    }

    [Theory]
    [MemberData(nameof(IntDatas2), 5, 8)]
    public void InterleaveTest(IEnumerable<int> first, IEnumerable<int> second)
    {
        first.Interleave(second).Should().Equal(0, 10, 1, 11, 2, 12, 3, 13, 4, 14, 15, 16, 17);
        first.Interleave(second, truncateRest: true).Should().Equal(0, 10, 1, 11, 2, 12, 3, 13, 4, 14);
        second.Interleave(first).Should().Equal(10, 0, 11, 1, 12, 2, 13, 3, 14, 4, 15, 16, 17);
        second.Interleave(first, truncateRest: true).Should().Equal(10, 0, 11, 1, 12, 2, 13, 3, 14, 4);
    }

    [Theory]
    [MemberData(nameof(IntDatas))]
    public void IntersperseTest(IEnumerable<int> source)
    {
        source.Intersperse(-1).Should().Equal(0, -1, 1, -1, 2, -1, 3, -1, 4, -1, 5, -1, 6, -1, 7);
    }

    [Theory]
    [MemberData(nameof(IntDatas))]
    public void PopFrontTest(IEnumerable<int> source)
    {
        source.PopFront(5, out var leadings).Should().Equal(5, 6, 7);
        leadings.Should().Equal(0, 1, 2, 3, 4);

        source.PopFirst(out var first).Should().Equal(1, 2, 3, 4, 5, 6, 7);
        first.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(IntDatas), 5)]
    public void RepeatTest(IEnumerable<int> source)
    {
        source.Repeat(2).Should().Equal(0, 1, 2, 3, 4, 0, 1, 2, 3, 4);
        source.RepeatInterleave(3).Should().Equal(0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4);
    }

    [Theory]
    [MemberData(nameof(IntDatas))]
    public void RotateTest(IEnumerable<int> source)
    {
        source.Rotate(5).Should().Equal(5, 6, 7, 0, 1, 2, 3, 4);
        source.Rotate(^3).Should().Equal(5, 6, 7, 0, 1, 2, 3, 4);
        source.Rotate(^9).Should().Equal(0, 1, 2, 3, 4, 5, 6, 7);
        source.Rotate(^0).Should().Equal(0, 1, 2, 3, 4, 5, 6, 7);
    }

    [Theory]
    [MemberData(nameof(IntDatas))]
    public void AtOrDefaultTest(IEnumerable<int> source)
    {
        source.TryAt(5, out var val).Should().BeTrue();
        val.Should().Be(5);

        source.TryAt(-1, out _).Should().BeFalse();
        source.TryAt(8, out _).Should().BeFalse();

        source.TryAt(^5, out val).Should().BeTrue();
        val.Should().Be(3);
        source.TryAt(^8, out val).Should().BeTrue();
        val.Should().Be(0);
        source.TryAt(^9, out _).Should().BeFalse();
    }

    public static TheoryData<IEnumerable<int>> IntDatas(int length)
        => new(EnumerateInts(length), ArrayInts(length), ListInts(length));

    public static TheoryData<IEnumerable<int>, IEnumerable<int>> IntDatas2(int len0, int len1)
        => new()
        {
            { EnumerateInts(len0), EnumerateInts(len1, 10) },
            { ArrayInts(len0), ArrayInts(len1, 10) },
            { ListInts(len0), ListInts(len1, 10) }
        };

    public static TheoryData<IEnumerable<int>> IntDatas() => IntDatas(8);

    private static IEnumerable<int> EnumerateInts(int length = 8, int start = 0)
    {
        for (int i = 0; i < length; i++) {
            yield return i + start;
        }
    }

    private static int[] ArrayInts(int length = 8, int start = 0)
    {
        var array = new int[length];
        for (int i = 0; i < length; i++) {
            array[i] = i + start;
        }
        return array;
    }

    private static List<int> ListInts(int length = 8, int start = 0)
    {
        var list = new List<int>();
        for (int i = 0; i < length; i++) {
            list.Add(i + start);
        }
        return list;
    }
}
