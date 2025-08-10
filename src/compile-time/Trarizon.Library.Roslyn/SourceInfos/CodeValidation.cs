using System;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Roslyn.SourceInfos;
public static class CodeValidation
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
}
