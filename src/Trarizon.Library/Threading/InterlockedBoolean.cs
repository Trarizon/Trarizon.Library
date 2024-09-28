using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Threading;
public struct InterlockedBoolean(bool value) : IEquatable<InterlockedBoolean>
{
    internal const int TrueValue = 1;
    internal const int FalseValue = 0;

    private int _value = value ? TrueValue : FalseValue;

    public readonly bool Value => _value != 0;

    /// <summary>
    /// Compare <see cref="Value"/> and <paramref name="comparand"/>, 
    /// if equals, replace with <paramref name="value"/>
    /// </summary>
    /// <returns>The original value</returns>
    public bool CompareExchange(bool value, bool comparand)
    {
        return FalseValue != Interlocked.CompareExchange(
            ref Unsafe.As<InterlockedBoolean, int>(ref this),
            value ? TrueValue : FalseValue, comparand ? TrueValue : FalseValue);
    }

    /// <summary>
    /// Set value as atomic operation
    /// </summary>
    /// <returns>The original value</returns>
    public bool Exchange(bool value)
    {
        return FalseValue != Interlocked.Exchange(
            ref Unsafe.As<InterlockedBoolean, int>(ref this),
            value ? TrueValue : FalseValue);
    }

    /// <summary>
    /// Value = !Value
    /// </summary>
    public void Toggle()
    {
        ref var self = ref Unsafe.As<InterlockedBoolean, int>(ref this);
        Interlocked.Exchange(ref self, self ^ TrueValue);
    }

    public static implicit operator InterlockedBoolean(bool boolean) => new(boolean);
    public static implicit operator bool(InterlockedBoolean interlocked) => interlocked.Value;
    public static bool operator true(InterlockedBoolean interlocked) => interlocked;
    public static bool operator false(InterlockedBoolean interlocked) => !interlocked;
    public static InterlockedBoolean operator !(InterlockedBoolean interlocked) => new(!interlocked.Value);
    public static bool operator ==(InterlockedBoolean left, InterlockedBoolean right) => left._value == right._value;
    public static bool operator !=(InterlockedBoolean left, InterlockedBoolean right) => left._value != right._value;

    public readonly bool Equals(InterlockedBoolean other) => this == other;
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is InterlockedBoolean b && Equals(b);
    public override readonly int GetHashCode() => _value.GetHashCode();
    public override readonly string ToString() => Value.ToString();
}
