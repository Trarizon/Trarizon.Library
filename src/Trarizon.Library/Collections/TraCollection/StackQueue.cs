using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections;
public static partial class TraCollection
{
    public static ReversedSpan<T> AsSpan<T>(Stack<T> stack)
        => Utils<T>.GetUnderlyingArray(stack).AsSpan(..stack.Count).AsReversed();
}
