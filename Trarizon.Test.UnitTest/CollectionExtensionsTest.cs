using Trarizon.Library.Collections.Extensions;

namespace Trarizon.Test.UnitTest;
[TestClass]
public class CollectionExtensionsTest
{
    [TestMethod]
    public void OffsetOfTest()
    {
        var ints = ArrayInts();

        ref readonly var forth = ref ints[3];
        Assert.AreEqual(ints.AsSpan().OffsetOf(in forth), 3);
        Assert.AreEqual(ints.OffsetOf(in forth), 3);
    }
}
