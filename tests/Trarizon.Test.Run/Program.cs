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
using Trarizon.Library.Buffers.Pooling;
using Trarizon.Library.CodeAnalysis;
using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.CodeGeneration;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Generic;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Text;
using Trarizon.Library.Text.Json;
using Trarizon.Library.Threading;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;

Console.WriteLine("Hello, world");

Stack<int> ints= new Stack<int>();
ints.Push(0);
ints.Push(1);
ints.Push(2);
ints.Push(3);
#if NET9_0_OR_GREATER
foreach (var item in TraCollection.AsSpan(ints).Reverse()) {
    item.Print();
}
#endif
namespace A
{
    [Singleton]
    partial class Proj
    {
    }
}


class Disposable : IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
    }
}

static class E
{
    public static void A(this Span<int> span) { }

    public static void A(this ReadOnlySpan<int> span) { }
}