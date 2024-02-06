// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Creators;
using Trarizon.Library.Collections.Extensions;
using Trarizon.Library.Learn.SourceGenerator;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;
using Trarizon.Test.UnitTest;

AllocOptStack<int> ints = [1, 2, 3, 5, 6];

ints.ToArray().Print();

ints.Push(1);
ints.ToArray().Print();
ints.Pop();
ints.AsEnumerable().Print();



[Singleton(InstanceProperty = "Nam")]
sealed partial class Si : IReadOnlyList<int>
{
    public int Count => throw new NotImplementedException();

    public int this[int index] => throw new NotImplementedException();


    private Si() { }

    void Test()
    {
    }

    public IEnumerator<int> GetEnumerator() => throw new NotImplementedException();
    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
}

partial class Si : IList<int>
{
    int IList<int>.this[int index]
    {
        get => this[index];
        set => throw new NotImplementedException();
    }

    int ICollection<int>.Count => throw new NotImplementedException();

    bool ICollection<int>.IsReadOnly => throw new NotImplementedException();

    void ICollection<int>.Add(int item) => throw new NotImplementedException();
    void ICollection<int>.Clear() => throw new NotImplementedException();
    bool ICollection<int>.Contains(int item) => throw new NotImplementedException();
    void ICollection<int>.CopyTo(int[] array, int arrayIndex) => throw new NotImplementedException();
    IEnumerator<int> IEnumerable<int>.GetEnumerator() => throw new NotImplementedException();
    int IList<int>.IndexOf(int item) => throw new NotImplementedException();
    void IList<int>.Insert(int index, int item) => throw new NotImplementedException();
    bool ICollection<int>.Remove(int item) => throw new NotImplementedException();
    void IList<int>.RemoveAt(int index) => throw new NotImplementedException();
}