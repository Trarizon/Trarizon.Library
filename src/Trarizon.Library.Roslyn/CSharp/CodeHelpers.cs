using System;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Roslyn.CSharp;
public static class CodeHelpers
{
    public static bool IsValidIdentifier([NotNullWhen(true)] ReadOnlySpan<char> identifier, bool allowAtPrefix = false)
    {
        if (identifier.IsEmpty)
            return false;

        var first = identifier[0];
        if (allowAtPrefix && first == '@')
            identifier = identifier[1..];

        if (first is not ('_' or >= 'a' and <= 'z' or >= 'A' and <= 'Z'))
            return false;

        foreach (var c in identifier[1..]) {
            if (c is not ('_' or >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9'))
                return false;
        }

        return true;
    }

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
