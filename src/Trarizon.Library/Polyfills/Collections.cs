using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library;

#if NETSTANDARD2_0

internal static partial class Polyfills
{
    public static bool TryPop<T>(this Stack<T> stack, [MaybeNullWhen(false)] out T item)
    {
        if (stack.Count > 0)
        {
            item = stack.Pop();
            return true;
        }
        item = default;
        return false;
    }
}

#endif
