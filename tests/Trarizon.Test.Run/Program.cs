// See https://aka.ms/new-console-template for more information
#pragma warning disable CS8500

using BenchmarkDotNet.Running;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
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
using Trarizon.Library.CodeTemplating.TaggedUnion;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Helpers;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;

ArrayInts().Distinct();

Console.WriteLine("Hello, World!");
return;

void Do(ref long l)
{
    Unsafe.As<long, (int, int)>(ref l) = (1, 2);
}


partial class Program
{
}
