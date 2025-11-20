using FluentAssertions;
using Trarizon.Library.Collections;

namespace Trarizon.Test.UnitTests.Collections;

public class ExtensionsTests
{
    [Fact]
    public void OffsetOfTest()
    {
        var ints = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };

        ref readonly var forth = ref ints.AsSpan()[3];
        forth.Should().Be(3);
        ints.AsSpan().OffsetOf(in forth).Should().Be(3);
    }
}
