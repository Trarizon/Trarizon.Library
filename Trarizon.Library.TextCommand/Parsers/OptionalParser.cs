using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Wrappers;
using Trarizon.TextCommand.Input;
using Trarizon.TextCommand.Parsers;

namespace Trarizon.Library.TextCommand.Parsers;
public readonly struct OptionalParser<T, TParser>(TParser parser) : IArgParser<Optional<T>> where TParser : IArgParser<T>
{
    public bool TryParse(InputArg input, [MaybeNullWhen(false)] out Optional<T> result)
    {
        if (parser.TryParse(input, out var res)) {
            result = res;
            return true;
        }
        else {
            result = default;
            return false;
        }
    }
}
