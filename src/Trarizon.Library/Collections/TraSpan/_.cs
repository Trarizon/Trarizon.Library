using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;
public static partial class TraSpan
{
    public static Span<T> AsSpan<T>(ref T value)
    {
        return MemoryMarshal.CreateSpan(ref value, 1);
    }

    public static ReadOnlySpan<T> AsReadOnlySpan<T>(ref readonly T value)
    {
        return MemoryMarshal.CreateReadOnlySpan(in value, 1);
    }
}
