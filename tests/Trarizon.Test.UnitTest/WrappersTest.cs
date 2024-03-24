using Trarizon.Library.Wrappers;

namespace Trarizon.Test.UnitTest;
[TestClass]
public class WrappersTest
{
    [TestMethod]
    public void Optional_DefaultOrNonParamCtor()
    {
        Assert.IsFalse(new Optional<int>().HasValue);
        Assert.IsFalse(default(Optional<int>).HasValue);
    }

    [TestMethod]
    public void Optional_Flattern()
    {
        var opt = Optional.Of(Optional.Of(1));
        Assert.IsTrue(opt.Value.HasValue);
        opt = Optional.Of(default(Optional<int>));
        Assert.IsFalse(opt.Value.HasValue);
        opt = default(Optional<Optional<int>>);
        Assert.IsFalse(opt.Value.HasValue);
    }
}
