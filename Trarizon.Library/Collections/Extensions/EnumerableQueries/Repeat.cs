﻿using Trarizon.Library.Collections.Extensions.Helpers.Queriers;

namespace Trarizon.Library.Collections.Extensions;
partial class EnumerableQuery
{
    /// <summary>
    /// Repeat the sequence for <paramref name="count"/> times
    /// </summary>
    public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source, int count)
    {
        if (source is IList<T> list)
            return list.RepeatList(count);

        if (count == 0)
            return Enumerable.Empty<T>();
        else
            return new RepeatQuerier<T>(source, count);
    }

    private sealed class RepeatQuerier<T>(IEnumerable<T> source, int count) : SimpleEnumerationQuerier<T, T>(source)
    {
        public override bool MoveNext()
        {
            const int End = MinPreservedState - 1;

            switch (_state) {
                case -1:
                    _state = count; // _state = _restCount
                    goto NewEnumerator;
                case End:
                    return false;
                default:
                    goto Iterating;
            }

        NewEnumerator:
            _enumerator = _source.GetEnumerator();
            _state--;
        Iterating:
            if (_enumerator!.MoveNext()) {
                _current = _enumerator.Current;
                return true;
            }
            else if (_state == 0) {
                _state = End;
                return false;
            }
            else {
                _enumerator.Dispose();
                goto NewEnumerator;
            }
        }

        protected override EnumerationQuerier<T> Clone() => new RepeatQuerier<T>(_source, count);
    }
}
