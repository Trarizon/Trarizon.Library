using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.Extensions.Helpers;
internal unsafe static class UnsafeUtil
{
    internal static unsafe int SubstractRef<T>(ref readonly T left, ref readonly T right)
    {
#pragma warning disable CS8500
        fixed (T* rPtr = &right, lPtr = &left) {
            var res = (int)((nuint)lPtr - (nuint)rPtr) / sizeof(T);
            Debug.Assert(Unsafe.AreSame(in Unsafe.Subtract(ref Unsafe.AsRef(in left), res), in right));
            return res;
        }
#pragma warning restore CS8500
    }
}
