// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
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
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Helpers;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;

int a = -1;
uint u = (uint)a;
Console.WriteLine(u); ;


partial class Program
{
    partial class NEst
    {
        [GeneratedRegex("")]
        public partial Regex A();
    }
}
namespace N
{
    namespace M.S
    {
        partial class AB
        {
            partial interface IDE<T, TD> where TD : IEnumerable
            {
                [Singleton(InstancePropertyName = "DDd", SingletonProviderName = "Dd")]
                public sealed partial class A
                {
                    public int DD
                    {
                        get;
                        set;
                    }
                }
            }
        }
    }
}
