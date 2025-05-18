using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Roslyn.CSharp;
public static class CodeValidation
{
    public static bool IsValidIdentifier([NotNullWhen(true)] string? identifier, bool allowAtPrefix = false)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return false;

        var first = identifier![0];
        if (!allowAtPrefix && first is '@')
            return false;

        if (char.IsDigit(first))
            return false;

        foreach (var c in identifier) {
            if (c is not ('_' or >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9'))
                return false;
        }
        return true;
    }
}
