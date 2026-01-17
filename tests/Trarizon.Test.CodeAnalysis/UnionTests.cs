using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Trarizon.Library.Functional.Attributes;

namespace Trarizon.Test.CodeAnalysis
{

    internal class UnionTests
    {
        public static void Test()
        {
            U u = 15L;

            u.TryAs<long>(out var s);

            var ui = u.As<int>();


            Console.WriteLine(s);
            Console.WriteLine(ui);
        }

        static void A(U u)
        {
            var i = u.As<int>();

            Console.WriteLine(i);
        }

        static void B(U u)
        {
            u.TryAs<long>(out var l);

            Console.WriteLine(l);
        }
    }

    [TypeUnion(typeof(ReadOnlySpan<char>), typeof(string), typeof(IEnumerable<string>), typeof(int), typeof(long), typeof(DateTime), typeof(CancellationToken), typeof(List<int>), typeof(ValueTuple<int, string>))]
    //[TypeUnion(typeof(string),typeof(int))]
    public readonly partial struct U
    {
    }

    [TypeUnion(typeof(List<string>), typeof(Stack<string>),
        ShareInterface = UnionShareInterfaceOption.Enabled)]
    public readonly partial struct V
    {

    }

    [TypeUnion(typeof(int), typeof(float),
        ShareInterface = UnionShareInterfaceOption.Enabled)]
    public readonly partial struct W
    {

    }

    [TypeUnion(typeof(A), typeof(B),
        ShareInterface = UnionShareInterfaceOption.Enabled)]
    readonly partial struct X
    {

    }

    interface ICus
    {
        //static abstract int D();
        event Action? E;
    }

    class A : ICus
    {
        public event Action? E;

        //public static int D() => throw new NotImplementedException();
    }

    struct B : ICus
    {
        public event Action? E { add { E += value; } remove { } }

        //public static int D() => throw new NotImplementedException();
    }
}
