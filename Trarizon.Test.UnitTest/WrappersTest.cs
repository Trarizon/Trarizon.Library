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
        Assert.IsTrue(opt.GetValueOrDefault().HasValue);
        opt = Optional.Of(default(Optional<int>));
        Assert.IsFalse(opt.GetValueOrDefault().HasValue);
        opt = default(Optional<Optional<int>>);
        Assert.IsFalse(opt.GetValueOrDefault().HasValue);
    }

    [TestMethod]
    public void LazyInit_1()
    {
        int i = 114;
        LazyInit<int> lazy = new(() => i + 400);
        Assert.IsFalse(lazy.HasValue);
        lazy.Value.ToString();
        Assert.IsTrue(lazy.HasValue);
        Assert.AreEqual(lazy.Value, 514);
    }

    [TestMethod]
    public void LazyInit_2()
    {
        LazyInit<int, int> lazy = new(114, i => i + 400);
        Assert.IsFalse(lazy.HasValue);
        lazy.Value.ToString();
        Assert.IsTrue(lazy.HasValue);
        Assert.AreEqual(lazy.Value, 514);
    }
}
