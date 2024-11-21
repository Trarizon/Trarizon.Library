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
using Trarizon.Library.Numerics;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Text;
using Trarizon.Library.Text.Json;
using Trarizon.Library.Threading;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;

Console.WriteLine("Hello, world");
var a = new A("str", 15);
Console.WriteLine(a.Prop);
Console.WriteLine(a.S);
ArgumentNullException.ThrowIfNull(a);

Trie.Create(ArrayValues(i => i.ToString()));

class A
{
    public readonly string Field;
    public string Prop { get; }

    public readonly double R;
    public readonly double S;

    public A(string str, double r)
    {
        if (str == "str") {
            Field = "f";
            Prop = "p";
        }
        else {
            Field = "F";
            Prop = "P";
        }
        Field = "Field";
        Prop = "Prop";

        R = r;
        S = this.R * this.R * Math.PI;
    }
}