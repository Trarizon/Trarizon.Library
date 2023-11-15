using Trarizon.Library.Collections.Extensions.Query;

namespace Trarizon.Test.UnitTest;
[TestClass]
public class ListQueriesTest
{
    [TestMethod]
    public void AtOrDefaultTest()
    {
        var array = Array();
        Assert.AreEqual(array.AtOrDefault(5), 5);
        Assert.AreEqual(array.AtOrDefault(-1, -1), -1);
        Assert.AreEqual(array.AtOrDefault(8, -1), -1);
        Assert.AreEqual(array.AtOrDefault(^5), 8 - 5);
        Assert.AreEqual(array.AtOrDefault(^8, -1), -1);
        Assert.AreEqual(array.AtOrDefault(^9, -1), -1);
    }
}
