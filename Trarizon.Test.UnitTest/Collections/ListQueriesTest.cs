using Trarizon.Library.Collections.Extensions;

namespace Trarizon.Test.UnitTest.Collections;
[TestClass]
public class ListQueriesTest
{
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
