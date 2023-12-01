// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Creators;
using Trarizon.Library.Collections.Extensions;
using Trarizon.Library.Collections.Extensions.Query;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;
using Trarizon.Test.UnitTest;

ReadOnlySpan<char> chars = "abcdefghi";

chars.OffsetOf(in chars[5]).Print();

"end".Print();