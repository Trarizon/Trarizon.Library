// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Creators;
using Trarizon.Library.Collections.Extensions;
using Trarizon.Library.Learn.SourceGenerator;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;
using Trarizon.Test.UnitTest;

if (ArrayInts().Select(i => i.ToString()).TrySingleOrNone(out var val))
    Console.WriteLine(val.GetHashCode()); // no warn
else
    Console.WriteLine(val.GetHashCode()); // warn

if (ArrayInts().Select(i => i.ToString()).TrySingleOrNone(out var val2, ""))
    Console.WriteLine(val2.GetHashCode()); // no warn
else
    Console.WriteLine(val2.GetHashCode()); // no warn

if (ArrayInts().Select<int, string?>(i => i.ToString()).TrySingleOrNone(out var val3, ""))
    Console.WriteLine(val3.GetHashCode()); // warn
else
    Console.WriteLine(val3.GetHashCode()); // warn

var rest = ArrayInts().Select(i => i.ToString()).PopFirst(out var first);
Console.WriteLine(first.GetHashCode()); // warn
var rest2 = ArrayInts().Select(i => i.ToString()).PopFirst(out var first2, "");
Console.WriteLine(first.GetHashCode()); // no warn
var rest3 = ArrayInts().Select<int, string?>(i => i.ToString()).PopFirst(out var first3);
Console.WriteLine(first.GetHashCode()); // warn
