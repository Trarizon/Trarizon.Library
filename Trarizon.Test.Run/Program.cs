// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Trarizon.Library.CodeAnalysis;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Creators;
using Trarizon.Library.Collections.Extensions;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;
using Trarizon.Test.UnitTest;

string? str = null;

NotNull<string> nstr = str;

Console.WriteLine(nstr.HasValue);
Console.WriteLine(Unsafe.IsNullRef(ref str));


[StructLayout(LayoutKind.Sequential)]
class RefType : IEnumerable<int>
{
    private readonly IEnumerable<int> _ints;

    public IEnumerator<int> GetEnumerator() => _ints.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_ints).GetEnumerator();
}