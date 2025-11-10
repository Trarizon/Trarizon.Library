using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Test.UnitTest.Collections;
[TestClass]
public class StackAllocTest
{
    [TestMethod]
    public void BitArray()
    {
        StackAllocBitArray bits = new(stackalloc uint[StackAllocBitArray.GetArrayLength(11)]);
        Assert.AreEqual(bits.Length, 16);

        bool[] comp = new bool[bits.Length];

        bits[3] = comp[3] = true;

        bits[8] = comp[8] = true;

        AssertSequenceEqual(ToArray(bits), comp);

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
