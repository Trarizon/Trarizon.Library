using FluentAssertions;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Test.UnitTests.Collections;

public class BitArrayTests
{
    [Theory]
    [InlineData(11, 32)]
    [InlineData(32, 32)]
    [InlineData(33, 64)]
    public void Test(int bitCount, int capacity)
    {
        var bits = new StackAllocBitArray(stackalloc uint[StackAllocBitArray.GetArrayLength(bitCount)]);
        bits.Length.Should().Be(capacity);

        bool[] array = new bool[bits.Length];
        bits[3] = array[3] = true;
        bits[8] = array[8] = true;

        ToArray(bits).Should().Equal(array);

        static bool[] ToArray(StackAllocBitArray bits)
        {
            bool[] bools = new bool[bits.Length];
            for (int i = 0; i < bits.Length; i++) {
                bools[i] = bits[i];
            }
            return bools;
        }
    }
}
