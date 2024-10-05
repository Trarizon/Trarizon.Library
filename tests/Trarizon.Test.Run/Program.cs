using BenchmarkDotNet.Running;
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
using System.Text.RegularExpressions;
using Trarizon.Library;
using Trarizon.Library.CodeAnalysis;
using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.CodeGeneration;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Generic;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Threading;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;

Console.WriteLine("Hello, world");

ReadOnlySpan<object> span = [1, 2,"55","st,", 3, 4, 5, 6, 8, 9];



namespace A
{
    [Singleton]
    partial class Proj
    {
        // private Proj() { }
    }
}


class Disposable : IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
        Console.WriteLine("Dispose");
    }
}