#pragma warning disable TRAEXP

using BenchmarkDotNet.Running;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Trarizon.Library;
using Trarizon.Library.Buffers.Pooling;
using Trarizon.Library.CodeAnalysis;
using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.CodeGeneration;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Generic;
using Trarizon.Library.Numerics;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Text;
using Trarizon.Library.Text.Json;
using Trarizon.Library.Threading;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;

Console.WriteLine("Hello, world");

foreach (var item in EnumerateInts().PopFront(1,out var lead)) {

}

static partial class C
{

    public static int M(string str, int anto) => str.Length;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    /// <param name="b"></param>
    /// <param name="v"></param>
    public static void M(ref readonly int s, [OptionalOut] out ValueTuple b, int v)
    {
        v = 0;
    }
}
