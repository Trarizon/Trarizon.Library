using System;

namespace Trarizon.Library.Roslyn.Emitting;

public static class EmittingHelpers
{
    /// <summary>
    /// Change path-invalid chars in csharpMemberName
    /// </summary>
    public static string ToFileNameString(string csharpMemberName)
    {
        bool changed = false;
        var builder = (stackalloc char[csharpMemberName.Length]);
        csharpMemberName.AsSpan().CopyTo(builder);
        foreach (ref var c in builder) {
            if (c is '<') {
                c = '{';
                changed = true;
            }
            else if (c is '>') {
                c = '}';
                changed = true;
            }
        }
        return changed ? builder.ToString() : csharpMemberName;
    }
}
