using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Wrappers;
using Trarizon.TextCommand.Parsers;

namespace Trarizon.Library.TextCommand.Parsers;
public readonly struct OptionalParser<T, TParser>(TParser parser) : IArgParser<Optional<T>> where TParser : IArgParser<T>
{
    public bool TryParse(ReadOnlySpan<char> rawArg, [MaybeNullWhen(false)] out Optional<T> result)
    {
        if (parser.TryParse(rawArg, out var res)) 
            result = res;
        else
            result = default;

        return true;
    }
}
