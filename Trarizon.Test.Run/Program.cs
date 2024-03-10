﻿// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Trarizon.Library;
using Trarizon.Library.CodeAnalysis;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Helpers;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;
using Trarizon.Test.UnitTest;

ArrayInts(4).CartesianProduct(ArrayInts(3)).Print();

ArrayInts(4).CartesianProductList(ArrayInts(3)).Print();

class A : IEnumerable<int>
{
    [FriendAccess(typeof(B))]
    internal static int _i;

    [FriendAccess()]
    IEnumerator<int> IEnumerable<int>.GetEnumerator() => throw new NotImplementedException();
    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
}

class B
{
    public int I
    {
        get {
            new A().AsEnumerable().GetEnumerator();
        }
    }
}

class C { public int I => A._i; }