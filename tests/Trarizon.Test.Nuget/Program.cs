// See https://aka.ms/new-console-template for more information


using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Trarizon.Library.Functional.Attributes;

Console.WriteLine("Hello, World!");
StringComparison comparison = default!;
// comparison.HasAnyFlag(StringComparison.OrdinalIgnoreCase);

// Optional<int> optional = 100;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
unsafe
{
    object* p = null;
    string* p2 = null;
    // p = p2;
}

#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
U u = default;


#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
unsafe ref T* GetPtr<T>(ref nint ptr)
{
    return ref Unsafe.As<nint, Ptr<T>>(ref ptr).Pointer;
}
unsafe struct Ptr<T> { public T* Pointer; }


[TypeUnion(typeof(void), typeof(int), typeof(Span<IEnumerable<char>>), typeof(decimal), typeof(string), typeof(IEnumerable), typeof(void*), typeof(string*), typeof(JsonElement))]
partial struct U
{
}

ref struct Abbb
{
    public int Value;
}

struct FP<T>
{
    public unsafe delegate*<int> Pointer;

    private nint _ptr;

    public unsafe ref T* PointerREf()
    {
        return ref *(T**)Unsafe.AsPointer<nint>(ref _ptr);
    }
}
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

// [Singleton]
partial class Proj
{

}