namespace Trarizon.Library.Roslyn;

internal static class Utils
{
    public static char? GetRightBracket(char left) => left switch
    {
        '{' => '}',
        '[' => ']',
        '(' => ')',
        '<' => '>',
        _ => null,
    };
}
