#pragma warning disable TRAEXP

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Trarizon.Library.CodeAnalysis;
using Trarizon.Library.CodeGeneration;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Generic;
using Trarizon.Library.Mathematics;
using Trarizon.Test.Run;

((Rational)0.5f).Print();
((Rational)60.5f).Print();
((Rational)1.25f).Print();
((Rational)0.2f).Print();

((Rational)1 / 5).Print();
//t.GetOrAdd("string");
//t.GetOrAdd("stringnext");
//t.GetOrAdd("bool");
//t.GetOrAdd("bolt");
//t.GetOrAdd("str");
//t.GetOrAdd("");

//Print();

//t.Root.IsEnd.Print();

//t.Remove("stringnext").Print();
//Print();

//t.Remove("").Print();

//Print();
//t.Root.IsEnd.Print();


//RunBenchmarks();
[Singleton]
partial class D
{
    private D()
    {
        Console.WriteLine(1);
    }
}

class A
{
    [FriendAccess(typeof(B))]
    public string Name { get; set; }
}

class B
{
    public static string Name => new A().Name;
}

class C
{
    //public static string Name => new A().Name;
}