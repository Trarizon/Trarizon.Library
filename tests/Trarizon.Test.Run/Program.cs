#pragma warning disable TRAEXP

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
using Trarizon.Library.Functional;
using System.Threading.Tasks;


Optional<Result<int, string>> x = Optional.Of(Result.Success<int, string>(1));

x.Transpose().ToString(true).Print();

x = Optional.None;
x.Transpose().ToString(true).Print();

x = Optional.Of(Result.Error<int, string>("Err"));
x.Transpose().ToString(true).Print();