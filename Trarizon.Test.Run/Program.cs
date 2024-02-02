// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Creators;
using Trarizon.Library.Collections.Extensions;
using Trarizon.Library.Learn.SourceGenerator;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;
using Trarizon.Test.UnitTest;

AllocOptList<int> list = [1, 2, 3];
list.Insert(2, 3);
list.AsSpan().Print();

[Singleton(InstanceProperty = "Nam")]
sealed partial class Si
{
	private Si() { }

	void Test()
	{
	}
}