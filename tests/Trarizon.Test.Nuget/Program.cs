// See https://aka.ms/new-console-template for more information


using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Trarizon.Library.Functional.Unions;

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

U u = default;


unsafe ref T* GetPtr<T>(ref nint ptr)
{
    return ref Unsafe.As<nint, Ptr<T>>(ref ptr).Pointer;
}

/// <summary>
/// <typeparamref name="T"/>
/// </summary>
/// <typeparam name="T"></typeparam>
unsafe struct Ptr<T> { public T* Pointer; }

/// <summary>
/// Test type union of <see cref="global::System.Span{char}"/> type
/// </summary>
[TypeUnion(typeof(void), typeof(string), typeof(IEnumerable), typeof(JsonElement), typeof(int), typeof(decimal),
    typeof(Span<IEnumerable<char>>), typeof(void*), typeof(string*), typeof(ReadOnlySpan<char>*), typeof(int**))]
partial struct U
{
    /// <summary>
    /// haha <c>T*</c>
    /// <paramref name="type"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void A<T>(Type type){}
}

// struct Span<T>
// {
    
// }

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