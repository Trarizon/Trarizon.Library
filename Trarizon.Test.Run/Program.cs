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

var dict = new AllocOptDictionary<string, string>(10);
dict.TryAdd("str","d");
dict.TryAdd("str2","de");
dict.TryAdd("str3","df");
dict.TryAdd("str4","dg");
dict["str4"] = "dd";
dict["str5"] = "gg";
dict.Remove("str");

foreach (var item in dict) {
    Console.WriteLine(item);
}