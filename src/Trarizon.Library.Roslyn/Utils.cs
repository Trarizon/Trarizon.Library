global using static Trarizon.Library.Roslyn.Utils;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Roslyn;

internal static partial class Utils
{
    public static char? GetRightBracket(char left) => left switch
    {
        '{' => '}',
        '[' => ']',
        '(' => ')',
        '<' => '>',
        _ => null,
    };

    public static string Fmt(in DefaultInterpolatedStringHandler handler) => handler.ToStringAndClear();

    public static string Fmt(IFormatProvider? formatProvider, Span<char> buffer, [InterpolatedStringHandlerArgument(nameof(formatProvider), nameof(buffer))] in DefaultInterpolatedStringHandler handler) => handler.ToStringAndClear();
}
