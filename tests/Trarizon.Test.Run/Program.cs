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

RunBenchmarks();
return;

var un = Un.CreateA(1);
un.TryGetA(out var a).Print();
a.Print();
un.TryGetC(out int s, out var b).Print();
s.Print();
b.Print();
un.TryGetE(out var e).Print();
e.Print();

unsafe {
    //    //(int, object) val=(5,new());
    //    //var span = MemoryMarshal.CreateSpan(ref Unsafe.As<int, byte>(ref val.Item1), 16);
    //    //span.Print();
    //    var u2 = new U2();
    //    //u2.u.Print();
    //    //sizeof(U2).Print();
    //    //MemoryMarshal.CreateSpan(ref Unsafe.As<int, byte>(ref u2.a.Item1), 24).Print();
    //    //MemoryMarshal.CreateSpan(ref Unsafe.As<int, byte>(ref u2.u.Item1), 24).Print();
}

Console.WriteLine("Hello, World!");


partial class Program
{
    [GeneratedRegex("")]
    public static partial Regex Ted();

    /// <summary>
    /// <code>
    /// union Un
    /// {
    ///     not     is 
    /// }
    /// </code>
    /// <see cref="int"/> is not
    /// </summary>
    /// <returns></returns>
    [UnionTag]
    enum UnKind
    {
        Zero,
        [TagVariant<int>("Str")]
        A,
        [TagVariant<int, string, string, int>("v", "dd", null, null)]
        B,
        [TagVariant(typeof(int), typeof((string, string Right)), Identifiers = ["s", "b"])]
        C,
        [TagVariant<string>("St")]
        D,
        [TagVariant<Program>("Prog")]
        E,
    }

    [StructLayout(LayoutKind.Explicit)]
    struct U3
    {
        [FieldOffset(0)]
        (int, object, object) t;
        [FieldOffset(0)]
        object obj; // 如果只有obj和unmanaged type
        [FieldOffset(0)]
        (object, long, int, object) a;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct U2
    {
        [FieldOffset(0)]
        public (int, int, int, int, CancellationToken) a;
        [FieldOffset(0)]
        public (int, int, int) u;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct U
    {
        [FieldOffset(0)]
        public A a;
        [FieldOffset(0)]
        public B b;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct A
    {
        public uint num;
        public string name;
        public List<U> list;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct B
    {
        public Queue<U> queue;
        public uint num;
        public string name;
    }
}
