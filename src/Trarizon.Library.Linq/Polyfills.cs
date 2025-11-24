using System.Runtime.CompilerServices;

namespace Trarizon.Library.Linq;

#if NETSTANDARD2_0

internal static class Polyfills
{
    extension(RuntimeHelpers)
    {
        public static bool IsReferenceOrContainsReferences<T>() 
            => !typeof(T).IsPrimitive;
    }

    public static bool TryPeek<T>(this Stack<T> stack, out T value)
    {
        if (stack.Count == 0) {
            value = default!;
            return false;
        }
        value = stack.Peek();
        return true;
    }

    public static bool TryDequeue<T>(this Queue<T> queue, out T value)
    {
        if (queue.Count == 0) {
            value = default!;
            return false;
        }
        value = queue.Dequeue();
        return true;
    }
}

#endif
