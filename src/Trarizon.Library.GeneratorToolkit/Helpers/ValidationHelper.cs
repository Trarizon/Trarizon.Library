using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit.Helpers;
public static class ValidationHelper
{
    public static bool IsValidIdentifier([NotNullWhen(true)] string? identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return false;

        if (char.IsDigit(identifier![0]))
            return false;

        foreach (var c in identifier) {
            if (c is not ('_' or (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9')))
                return false;
        }

        return true;
    }
}
