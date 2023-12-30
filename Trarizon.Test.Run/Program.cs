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

Optional<int> opt = default;

if (opt is (false, var val)) {

}

"end".Print();

[Singleton(InstanceProperty = "Nam")]
sealed partial class Si
{
    private Si() { }

    void Test()
    {
    }
}