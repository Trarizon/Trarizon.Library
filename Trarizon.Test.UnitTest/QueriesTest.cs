using Trarizon.Library.Collections.Extensions;
using Trarizon.Library.Wrappers;

namespace Trarizon.Test.UnitTest;
[TestClass]
public class EnumerableQueriesTest
{
    [TestMethod]
    public void WhereSelectTest()
    {
        var array = Enumerate();

        AssertSequenceEqual(array.WhereSelect((x) => x % 2 == 0 ? default : Optional.Of(x.ToString())),
            "1", "3", "5", "7");
    }

    [TestMethod]
    public void PopFrontTest()
    {
        var array = Enumerate();
        AssertSequenceEqual(array.PopFront(5, out var leadings), 5, 6, 7);
        AssertSequenceEqual(leadings, 0, 1, 2, 3, 4);
        AssertSequenceEqual(array.PopFirst(out var first), 1, 2, 3, 4, 5, 6, 7);
        Assert.AreEqual(first, 0);
        AssertSequenceEqual(1, array.PopFrontWhile(out leadings, i => i < 3), 3, 4, 5, 6, 7);
        AssertSequenceEqual(leadings, 0, 1, 2);

        array = Array();
        AssertSequenceEqual(array.PopFront(5, out leadings), 5, 6, 7);
        AssertSequenceEqual(leadings, 0, 1, 2, 3, 4);
        AssertSequenceEqual(array.PopFirst(out first), 1, 2, 3, 4, 5, 6, 7);
        Assert.AreEqual(first, 0);
        AssertSequenceEqual(2, array.PopFrontWhile(out leadings, i => i < 3), 3, 4, 5, 6, 7);
        AssertSequenceEqual(leadings, 0, 1, 2);
    }

    [TestMethod]
    public void RepeatTest()
    {
        var array = Enumerate(5);
        AssertSequenceEqual<int>(array.Repeat(2), [.. array, .. array]);
        
        array = Array(5);
        AssertSequenceEqual<int>(array.Repeat(2), [.. array, .. array]);
    }
}