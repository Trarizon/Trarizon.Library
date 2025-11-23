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

    [Fact]
    public void StackAtTest()
    {
        var stack = new Stack<int>();
        stack.Push(0);
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        stack.Push(4);
        stack.Push(5);

        stack.At(0).Should().Be(5);
        stack.At(1).Should().Be(4);
        stack.At(2).Should().Be(3);
        stack.At(3).Should().Be(2);
        stack.At(4).Should().Be(1);
        stack.At(5).Should().Be(0);
    }

    [Fact]
    public void QueueAtTest()
    {
        var queue = new Queue<int>();
        queue.Enqueue(0);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);

        queue.At(0).Should().Be(0);
        queue.At(1).Should().Be(1);
        queue.At(2).Should().Be(2);

        queue.Dequeue();
        queue.Dequeue();
        queue.Enqueue(4);
        queue.Enqueue(5);

        queue.At(0).Should().Be(2);
        queue.At(1).Should().Be(3);
        queue.At(2).Should().Be(4);
        queue.At(3).Should().Be(5);

        TraCollection.UnsafeAccess<int>.GetHead(queue).Should().Be(2);

        var action = () => queue.At(4);
        action.Should().Throw<ArgumentOutOfRangeException>();

        queue.Dequeue();
        queue.Dequeue();
        queue.At(0).Should().Be(4);
        queue.At(1).Should().Be(5);
    }
}
