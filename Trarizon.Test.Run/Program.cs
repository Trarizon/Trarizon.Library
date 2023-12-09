// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Creators;
using Trarizon.Library.Collections.Extensions;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;
using Trarizon.Test.UnitTest;

int[,] ints = {
    {1,2,3 },
    {4,5,6 },
    {7,8,9 },
    {10,11,12 },
    {13,14,15 },
};

ints.AsSpan(1).Print();

"end".Print();
