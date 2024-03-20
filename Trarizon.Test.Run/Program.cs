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


AllocOptDeque<int> deque = [];

void Display() 
{
    deque.Print();
    deque.GetUnderlyingArray().Print();
    Console.WriteLine();
}


namespace A.A2
{
    namespace N
    {
        partial class W<T, T2> : Collection<T>
            where T : List<T>
        {
            [Singleton(SingletonProviderName = "S",Options = SingletonOptions.IsInternalInstance)]
            sealed partial class A
            {
                // private A(int a) { }

                [BackingFieldAccess(nameof(Field))]
                private string _field;

                public string Field
                {
                    [MemberNotNull(nameof(_field))]
                    init {
                        _field = "";
                    }
                }
            }
        }
    }
}