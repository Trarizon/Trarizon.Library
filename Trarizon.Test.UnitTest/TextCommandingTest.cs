using Trarizon.TextCommanding;
using Trarizon.TextCommanding.Attributes;
using Trarizon.TextCommanding.Input;

namespace Trarizon.Test.UnitTest;
[TestClass]
public class TextCommandingTest
{
    [TestMethod]
    public void ParseArgsTest()
    {
        Args args = TCExecution.ParseArgs<Args>("""--property 1 -ip "initp ""escape"" and space" -fc -2 0 1 2 3 4 5 --f -1 6 7 8 9 0 1 2 3 4 5 6 7 8 9""");
        Assert.IsNotNull(args);
        Assert.AreEqual(args.Property, 1);
        Assert.AreEqual(args.InitProperty, "initp \"escape\" and space");
        Assert.AreEqual(args.PrivateField, -1);
        Assert.AreEqual(args.FromConstructor, -2);
        AssertSequenceEqual(args.IList, "0", "1", "2", "3");
        Assert.AreEqual(args.Value, 4);
        AssertSequenceEqual(args.List, "5", "6", "7");
        AssertSequenceEqual(args.Enumerable, "8", "9", "0");
        AssertSequenceEqual(args.Array, 1, 2, 3, 4, 5, 6, 7, 8, 9);
    }

    [TestMethod]
    public void SplitArgsTest()
    {
        StringSplitArgs args = """" -prop "-prop in ""str""" 1  --val 2"""".SplitAsArguments();
        AssertSequenceEqual(args.ToList(), "-prop", "-prop in \"str\"", "1", "--val", "2");
    }

    class Args
    {
        // Parameter cannot be readonly or get-only

        [TCOption("property")]
        public int Property { get; set; }
        [TCOption("init-property", "ip")]
        public string InitProperty { get; init; }

        [TCOption("f")]
        private int _privateField;
        public int PrivateField => _privateField;

        // Order is required, unless there's only one [TCValue] or [TCValues]
        // [TCValue] will call T.Parse(string)
        // [TCValues] will auto-create collections

        [TCValues(order: 2, 3)]
        private IEnumerable<string> _enumerable;
        public IEnumerable<string> Enumerable => _enumerable;

        [TCValue(0)]
        public int Value { get; set; }

        [TCValues(-1, 4)]
        private IList<string> _ilist;
        public IList<string> IList => _ilist;

        [TCValues(1, 3)]
        private List<string> _list;
        public List<string> List => _list;

        [TCValues(3)]
        private int[] _array;
        public int[] Array => _array;

        public int FromConstructor { get; set; }

#pragma warning disable CS8618 
        // If there's only one ctor, [TCConstructor] is not required
        [TCConstructor]
        public Args([TCOption("fromCtor", "fc")] int fromCtor)
        {
            FromConstructor = fromCtor;
        }

        public Args() { }
#pragma warning restore CS8618
    }
}
