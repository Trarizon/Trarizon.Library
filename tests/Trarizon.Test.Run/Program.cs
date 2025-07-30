#pragma warning disable TRAEXP

using Microsoft.CodeAnalysis.CSharp;
using BenchmarkDotNet.Filters;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Generic;
using Trarizon.Library.Mathematics;
using Trarizon.Test.Run;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Runtime.CompilerServices;
using Trarizon.Library.Functional;
using System.Threading.Tasks;
using Trarizon.Library.CodeAnalysis.Generation;
using Trarizon.Library.CodeAnalysis.Diagnostics;


var res = Result.Error(StringComparison.OrdinalIgnoreCase).Build<int>();

res.Print();

_ = S.Instance.T();

[Singleton]
partial class S
{
    [FriendAccess(typeof(F))]
    public string T() => ToString();
}

class F
{
    public static string Create() => S.Instance.T();
}