using System.Runtime.CompilerServices;

namespace Trarizon.Library;
public static class TraUnsafe
{
    public static ref readonly TTo AsReadOnly<TFrom, TTo>(ref readonly TFrom source)
        => ref Unsafe.As<TFrom, TTo>(ref Unsafe.AsRef(in source));
}
