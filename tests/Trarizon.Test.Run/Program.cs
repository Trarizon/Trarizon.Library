// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Trarizon.Library;
using Trarizon.Library.CodeAnalysis;
using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.CodeTemplating;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Helpers;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;

//RunBenchmarks();
new D().Count.Print();
(new D() as IInter).Count.Print();
//ArrayValues(i => i.ToString(), 2).CartesianProductList(ArrayInts(4)).Print();

interface IInter
{
    int Count { get; }
}

class B : IInter
{
    public int Count => 1;

    Regex Gen() => default!;
}

class D : B, IInter
{
    int IInter.Count => 2;
}

//namespace A.A2
//{
//    public partial class B
//    {
//        [FriendAccess(typeof(List<int>))]
//        internal partial string Field();
//    }

//    [Singleton]
//    sealed partial class B
//    {
//        [FriendAccess(typeof(IDictionary<,>), Options = FriendAccessOptions.AllowInherits)]
//        private B() { }

//        internal partial string Field() { return ""; }
//    }
//}