using Trarizon.Library.Collections;
using Trarizon.Library.Linq;

namespace Trarizon.Test.UnitTest.Collections;
[TestClass]
public class EnumerableQueriesTest
{
    [TestMethod]
    public void PopFrontTest()
    {
        var array = EnumerateInts();
        AssertSequenceEqual(array.PopFront(5, out var leadings), 5, 6, 7);
        AssertSequenceEqual(leadings, 0, 1, 2, 3, 4);
        AssertSequenceEqual(array.PopFirst(out var first), 1, 2, 3, 4, 5, 6, 7);
        Assert.AreEqual(first, 0);

        array = ArrayInts();
        AssertSequenceEqual(array.PopFront(5, out leadings), 5, 6, 7);
        AssertSequenceEqual(leadings, 0, 1, 2, 3, 4);
        AssertSequenceEqual(array.PopFirst(out first), 1, 2, 3, 4, 5, 6, 7);
        Assert.AreEqual(first, 0);
    }

    [TestMethod]
    public void RepeatTest()
    {
        var array = EnumerateInts(5);
        AssertSequenceEqual(array.Repeat(2), [.. array, .. array]);

        array = ArrayInts(5);
        AssertSequenceEqual(array.Repeat(2), [.. array, .. array]);
    }

    [TestMethod]
    public void AtOrDefaultTest()
    {
        var array = ArrayInts();
        Assert.IsTrue(array.TryAt(5, out var val));
        Assert.AreEqual(val, 5);
        Assert.IsFalse(array.TryAt(-1, out _));
        Assert.IsFalse(array.TryAt(8, out _));
        Assert.IsTrue(array.TryAt(^5, out val));
        Assert.AreEqual(val, 8 - 5);
        Assert.IsFalse(array.TryAt(^8, out _));
        Assert.IsFalse(array.TryAt(^9, out _));
    }

}