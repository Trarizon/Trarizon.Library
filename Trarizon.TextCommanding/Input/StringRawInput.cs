namespace Trarizon.TextCommanding.Input;
internal struct StringRawInput(ReadOnlyMemory<char> input) : IRawInput
{
    private int _index = -1;
    private int _spanEnd = -1;
    private bool _escaped;

    public readonly InputSplit Current => new(input.Span[_index.._spanEnd], _escaped);

    public bool MoveNext()
    {
        _index = _spanEnd + 1;
        if (_index >= input.Length)
            return false;

        var span = input.Span;

        while (char.IsWhiteSpace(span[_index]))
            _index++;

        // Value "in quote"
        if (span[_index] == '"') {
            _spanEnd = TextCommandUtility.FindFirstRightQuote(span, ++_index);
            _escaped = true;
        }
        else {
            _spanEnd = TextCommandUtility.FindFirstWhiteSpace(span, _index);
            _escaped = false;
        }

        return true;
    }
}

// Public in SplitAsArguments();
public readonly struct StringSplitArgs(ReadOnlyMemory<char> input)
{
    public readonly Enumerator GetEnumerator() => new(input);

    public readonly List<string> ToList()
    {
        var result = new List<string>();
        var enumerator = GetEnumerator();
        while (enumerator.MoveNext()) {
            var arg = enumerator.Current;
            result.Add(new string(arg));

        }
        foreach (var arg in this) {
        }
        return result;
    }

    public struct Enumerator(ReadOnlyMemory<char> input)
    {
        private StringRawInput _rawInput = new(input);

        public readonly string Current
        {
            get {
                return new(_rawInput.Current.Value);
            }
        }

        public bool MoveNext() => _rawInput.MoveNext();
    }
}
