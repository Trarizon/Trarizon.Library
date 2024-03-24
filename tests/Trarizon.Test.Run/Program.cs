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


var arr = ArrayInts().OfTypeUntil<string, string>();

ArrayInts().FirstByMaxPriorityOrDefault(StringComparison.Ordinal,
    (x) => (StringComparison)x);
// TODO: Explicit capture

Local();

void Local()
{
    arr.Take(1).ToList().Print();
}


partial class Program
{
    private int c;

    public void Deconstruct()
    {
        var a = 0;
        var b = 0;

        Local();
        a.Print();
        b.Print();
        AnotherLocal();
        c++;
        Another();
        //a.ToString().Select(Sel).Print();
        //var sel = Sel;
        //sel('0').Print();
        //var an = Another;
        //an.Print();

        char Sel(char c)
        {
            a++;
            return c;
        }

        int Local()
        {

            a++;
            b--;
            return a;
        }

        void AnotherLocal()
        {
            a++;
            b--;
            c += 22;
        }

        void Another()
        {
            c++;
        }
    }
}

//namespace A.A2
//{
//    public partial class B
//    {
//        [FriendAccess()]
//        public partial string Field();
//    }

//    [Singleton]
//    sealed partial class B
//    {
//        [FriendAccess]
//        private B() { }

//        [FriendAccess(typeof(List<>))]
//        protected internal string field
//        {
//            get;
//            [FriendAccess(typeof(List<>))]
//            set;
//        }

//        private Action<string> _onAct;
//        public event Action<string> OnAct
//        {
//            add => _onAct += value;
//            remove => _onAct -= value;
//        }

//        public partial string Field() { return ""; }
//    }

//    namespace N
//    {
//        partial class W<T, T2> : Collection<T>
//            where T : List<T>
//        {
//            [Singleton(SingletonProviderName = "S", Options = SingletonOptions.IsInternalInstance)]
//            sealed partial class A
//            {
//                // private A(int a) { }

//                [BackingFieldAccess(nameof(Field))]
//                private string _field;

//                public string Field
//                {
//                    [MemberNotNull(nameof(_field))]
//                    init {
//                        _field = "";
//                        var b = new B() {
//                            field = "str",
//                        };
//                        b.field ??= "str";

//                        b.OnAct += str => { };

//                        var func = b.Field;
//                    }
//                }
//            }
//        }
//    }
//}