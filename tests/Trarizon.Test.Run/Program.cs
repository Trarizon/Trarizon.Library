#pragma warning disable TRAEXP

using CommunityToolkit.HighPerformance;
using System.Runtime.CompilerServices;
using Trarizon.Library.Functional;

//RunBenchmarks();

var v = Optional.Of(Task.CompletedTask);

await v;

var v2 = Optional.Of(Task.FromResult(5));

await v2;



// 结论：
// 定义一个RefFunc，利用OverloadResolutionPriority和RefFunc定义两个重载。
// 只需要出结果的那个Func换成RefFunc？
// 另外提供一个Func的allows ref struct版本，提供不同的签名（比如SelectRef）。

// 当调用SelectRef时，如果传入的参数的TResult不是allows ref struct，发出warning。
// - 以上情况主要防的应该是使用var定义参数，导致推断结果是RefOptional。其他情况应该不需要警告？比如继续chain，会把不该是refopt还原会opt

Func<int, ReadOnlySpan<char>> f = x => x.ToString().AsSpan();

f.AsRefFunc().Invoke(123).Print();

int[] list = [1, 2, 3, 4];
Stack<int> stack = Unsafe.As<Stack<int>>(list);
stack.Pop().Print();
stack.Count.Print();
stack.PrintType();

123.M(x => x.ToString()).Print();
123.M(f.AsRefFunc()).Value.Print();

Func<int, ReadOnlySpan<char>, ReadOnlySpan<char>> f2 = (x, y) => x.ToString().AsSpan();

123.B(x => Optional.Of(x.ToString()), (x, y) => $"{x}{y}").Print();
123.B(F, (x, y) => $"{x}{y}").Value.Print();
123.B(x => Optional.Of(x.ToString().AsSpan()), (x, y) => $"{x}{y}").Value.Print();
123.B(x => Optional.Of(x.ToString().AsSpan()), f2.AsRefFunc()).Value.Print();

Optional.Of(123).Bind(F);
Optional.Of(123).Bind(F, (x, y) => $"{x}{y}").Print();

Optional<string> F(int x) => x.ToString();

delegate TResult RefFunc<in T, out TResult>(T arg)
    where T : allows ref struct
    where TResult : allows ref struct;

delegate TResult RefFunc<in T, in T2, out TResult>(T arg, T2 arg2)
    where T : allows ref struct
    where T2 : allows ref struct
    where TResult : allows ref struct;


static class A
{
    public static Optional<TR> M<T, TR>(this T v, Func<T, TR> func) where T : allows ref struct => func(v!);

    [OverloadResolutionPriority(-1)]
    public static RefOptional<TR> M<T, TR>(this T v, RefFunc<T, TR> func)
        where T : allows ref struct where TR : allows ref struct
        => func(v!);

    public static Optional<TR> B<T, T2, TR>(this T v, Func<T, RefOptional<T2>> func, Func<T, T2, TR> func2)
        where T : allows ref struct where T2 : allows ref struct => func2(v, func(v!).Value);

    public static Optional<TR> B<T, T2, TR>(this T v, Func<T, Optional<T2>> func, Func<T, T2, TR> func2)
        where T : allows ref struct => func2(v, func(v!).Value);

    [OverloadResolutionPriority(-1)]
    public static RefOptional<TR> B<T, T2, TR>(this T v, Func<T, RefOptional<T2>> func, RefFunc<T, T2, TR> func2)
        where T : allows ref struct where T2 : allows ref struct where TR : allows ref struct
        => func2(v, func(v!).Value);

    public static RefFunc<T, TResult> AsRefFunc<T, TResult>(this Func<T, TResult> func)
        where T : allows ref struct where TResult : allows ref struct
        => Unsafe.As<RefFunc<T, TResult>>(func);

    public static RefFunc<T, T2, TResult> AsRefFunc<T, T2, TResult>(this Func<T, T2, TResult> func)
        where T : allows ref struct where T2 : allows ref struct where TResult : allows ref struct
        => Unsafe.As<RefFunc<T, T2, TResult>>(func);
}
static class B
{
    //[OverloadResolutionPriority(-1)]
    //public static RefOptional<TR> M<T, TR>(this T v, Func<T, TR> func)
    //    where T : allows ref struct where TR : allows ref struct
    //    => func(default!);
}

