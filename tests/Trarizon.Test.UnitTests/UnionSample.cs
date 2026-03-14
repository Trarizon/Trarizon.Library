using System;
using System.Collections.Generic;
using System.Text;
using Trarizon.Library.Functional;

namespace Trarizon.Test.Run;

internal static class UnionSample
{
    public static void Run()
    {
        var union = Union.A("a", 1);
        var str = union switch
        {
            { A: (var d, var i) } => (d, i).ToString()
        };

    }

    extension(Union u)
    {
        public static Union A(string str, int i) { return default; }
    }
}

//[Union]
partial struct U
{

}

partial struct Um
{
    private int _flag;
    private object _ref0;
    public int _unmanaged0;

    public int Flag { get; }

    public object Cancellation { get; }
}

//[Union]
partial struct Union
{
    private int _flag;
    private object _ref0;
    private object _ref1;
    private CancellationToken _managed0;
    private CancellationToken _managed1;
    private int _unmanaged0;
    private int _unmanaged1;

    public int Flag => _flag;

    //[Variant]
    public partial (string Str, int Int) A { get; }

    //[Variant("Item")]
    //public partial string B { get; }

    //[Variant]
    //public partial Unit C { get; }

    public ref struct _Ref
    {
        ref Union _union;
    }

    public ref struct _Ref_A
    {
        ref Union _union;

    }
}

partial struct Union
{
    public partial (string Str, int Int) A { get => field; }
}