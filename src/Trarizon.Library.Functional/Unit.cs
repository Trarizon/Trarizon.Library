using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Functional;

#if UNIT

public readonly struct Unit : IEquatable<Unit>
{
    public static Unit Value => default;

    public bool Equals(Unit other) => true;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Unit;
    public override int GetHashCode() => 0;
    public static bool operator ==(Unit left, Unit right) => true;
    public static bool operator !=(Unit left, Unit right) => false;
}

#endif