using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections;

public static partial class TraCollection
{
    public static ReversedSpan<T> AsSpan<T>(Stack<T> stack)
        => UnsafeAccess<T>.GetUnderlyingArray(stack).AsSpan(0, stack.Count).AsReversed();

    /// <summary>
    /// Get the item at <paramref name="index"/>, from the top
    /// </summary>
    public static T At<T>(this Stack<T> stack, int index) 
        => UnsafeAccess<T>.GetUnderlyingArray(stack)[stack.Count - 1 - index];
}
