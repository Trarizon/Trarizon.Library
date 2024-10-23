using System.Runtime.CompilerServices;

namespace Trarizon.Library.Wrappers;
/// <summary>
/// A helper to use <see langword="using"/> statement easier, use
/// <see cref="LazyInitDisposableExt.Set{T}(ref readonly Trarizon.Library.Wrappers.LazyInitDisposable, T)"/>
/// to assign and get the value of the wrapped field
/// <para>
/// Normally if we want a lazy-init <see cref="IDisposable"/>, we should do this:
/// <code>
/// SomeDisposable? variable = null;
/// try { /* Do something */ } finally { variable?.Dispose(); }
/// </code>
/// We have to manually write try-finally here, unless we create a defer wrapper which
/// will create a closure of <c>variable</c>.
/// <br/>
/// With <see cref="LazyInitDisposable"/>, you can use <see langword="using"/> statement to do this,
/// not try-finally needed
/// <code>
/// using var scope = new DisposableScope();
/// var variable = scope.Set(CreateDisposableVariable());
/// // ...
/// </code>
/// But you cannot call set twice, or else the first value will not be disposed
/// </para>
/// </summary>
/// <typeparam name="T"></typeparam>
public struct LazyInitDisposable : IDisposable
{
    internal IDisposable? _value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Dispose() => _value?.Dispose();
}

public struct LazyInitDisposable<T> : IDisposable where T : IDisposable
{
    internal T? _value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Dispose() => _value?.Dispose();
}

public static class LazyInitDisposableExt
{
    /// <summary>
    /// Set the value return the reference of wrapped field of <paramref name="wrapper"/>
    /// <br/>
    /// DO NOT call this method twice on single <paramref name="wrapper"/> instance
    /// </summary>
    /// <param name="wrapper">
    /// Note that <see langword="ref readonly" /> on this paremter is just a workround to cheat compiler,
    /// the field of wrapper is still modified
    /// </param>
    /// <returns>reference of value of <paramref name="wrapper"/>, which has the same value with <paramref name="value"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Set<T>(this ref readonly LazyInitDisposable wrapper, T value) where T : class, IDisposable
    {
        // wrapper should be ref readonly because using variables cannot be modified.
        // so we have to use Unsafe.AsRef to assign the field, here modifying wrapper
        // is accepted
        ref var w = ref Unsafe.AsRef(in wrapper);
        w._value = value;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly T Set<T>(this ref readonly LazyInitDisposable<T> wrapper, T value) where T : IDisposable
    {
        Unsafe.AsRef(in wrapper)._value = value;
        return ref wrapper._value!;
    }
}