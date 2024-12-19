#if NETSTANDARD2_0

using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections;
partial class TraCollection
{
    public static bool TryPop<T>(this Stack<T> stack, [MaybeNullWhen(false)] out T value)
    {
        if (stack.Count == 0) {
            value = default;
            return false;
        }
        value = stack.Pop();
        return true;
    }
}

#endif
