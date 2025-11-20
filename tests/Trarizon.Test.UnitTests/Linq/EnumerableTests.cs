using FluentAssertions;
using Trarizon.Library.Linq;

namespace Trarizon.Test.UnitTests.Linq;

public class EnumerableTests
{
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
        source.Repeat(2).Should().Equal([0, 1, 2, 3, 4, 0, 1, 2, 3, 4]);
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
        => new(EnumerateInts(length), ArrayInts(length));

    public static TheoryData<IEnumerable<int>> IntDatas() => IntDatas(8);

    private static IEnumerable<int> EnumerateInts(int length = 8)
    {
        for (int i = 0; i < length; i++) {
            yield return i;
        }
    }

    private static int[] ArrayInts(int length = 8)
    {
        var array = new int[length];
        for (int i = 0; i < length; i++) {
            array[i] = i;
        }
        return array;
    }
}
