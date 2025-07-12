#pragma warning disable TRAEXP

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using BenchmarkDotNet.Filters;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Runtime.InteropServices;
using Trarizon.Library.CodeAnalysis;
using Trarizon.Library.CodeGeneration;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Generic;
using Trarizon.Library.Mathematics;
using Trarizon.Test.Run;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Runtime.CompilerServices;

dynamic d = new DynamicPath("D:\\");
d.Pictures.Hentai.剥ぎコラ.Open();

string str = "Hello, World!";

Unsafe.AsRef(in str.AsSpan()[0]) = 'a';

Span<int> ints= [];
ints.Contains(1);


((Rational)0.5f).Print();
((Rational)60.5f).Print();
((Rational)1.25f).Print();
((Rational)0.2f).Print();

((Rational)1 / 5).Print();

class DynamicPath(string path) : DynamicObject
{
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var name = binder.Name;
        var newpath = Path.Combine(path, name);
        if (Directory.Exists(newpath)) {
            result = new DynamicPath(newpath);
            return true;
        }
        if (File.Exists(newpath)) {
            result = new DynamicPath(newpath);
            return true;
        }
        result = null;
        return false;
    }

    public void Open()
    {
        if (Directory.Exists(path)) {
            Process.Start("explorer.exe", path);
            return;
        }
        if (File.Exists(path)) {
            Process.Start("explorer.exe", $"/select,{path}");
            return;
        }
    }
}