namespace Trarizon.Library.Net;

public static class HttpUrlHelpers
{
    public static ReadOnlySpan<char> GetQueryValue(ReadOnlySpan<char> queryParts, ReadOnlySpan<char> key)
    {
        var iterator = new QuerySpanIterator(queryParts);
        foreach (var pair in iterator) {
            if (pair.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                return pair.Value;
        }
        return [];
    }

    public static ReadOnlySpan<char> GetQueryPart(ReadOnlySpan<char> url)
    {
        var index = url.IndexOf('?');
        return index == -1 ? [] : url[(index + 1)..];
    }

    private ref struct QuerySpanIterator
    {
        private readonly ReadOnlySpan<char> _queryString;
        private int _start;
        private int _delimiter;
        private int _end;

        internal QuerySpanIterator(ReadOnlySpan<char> queryString)
        {
            if (queryString.Length > 0 && queryString[0] == '?')
                _queryString = queryString[1..];
            else
                _queryString = queryString;
        }

        public readonly QueryKeyValuePair Current => new(_queryString[_start.._end], _delimiter == -1 ? -1 : _delimiter - _start);

        public QuerySpanIterator GetEnumerator()
            => this with { _end = -1 };

        public bool MoveNext()
        {
            if (_end == _queryString.Length)
                return false;

            int delimiter = -1;
            int start = _end + 1;
            int index = start;
            for (; index < _queryString.Length; index++) {
                var ch = _queryString[index];
                if (delimiter == -1 && ch == '=') {
                    delimiter = index;
                }
                else if (ch == '&') {
                    break;
                }
            }
            _start = start;
            _delimiter = delimiter;
            _end = index;
            return true;
        }
    }

    private readonly ref struct QueryKeyValuePair
    {
        private readonly ReadOnlySpan<char> _pair;
        private readonly int _equalIndex;

        public QueryKeyValuePair(ReadOnlySpan<char> pair, int equalIndex)
        {
            _pair = pair;
            _equalIndex = equalIndex;
        }

        public ReadOnlySpan<char> Key => _equalIndex < 0 ? [] : _pair[0.._equalIndex];

        public ReadOnlySpan<char> Value => _pair[(_equalIndex + 1)..];
    }

}
