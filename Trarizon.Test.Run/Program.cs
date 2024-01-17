// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Creators;
using Trarizon.Library.Collections.Extensions;
using Trarizon.Library.Learn.SourceGenerator;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;
using Trarizon.Test.UnitTest;

Test<int>().Print();
Test<string>().Print();
Test<List<int>>().Print();

"end".Print();

static bool Test<T>()
{
    T[] res = new T[1];
    return res[0] is null;
}

[Singleton(InstanceProperty = "Nam")]
sealed partial class Si
{
    private Si() { }

    void Test()
    {
    }
}