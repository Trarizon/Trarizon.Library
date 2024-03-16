using Trarizon.Library.Collections.Helpers.Utilities.Queriers;

namespace Trarizon.Library.Collections.Helpers;
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
        else if (source.TryGetNonEnumeratedCount(out var colCount) && colCount == 0)
            return Enumerable.Empty<T>();
        else
            return new RepeatQuerier<T>(source, count);
    }

    /// <summary>
    /// Repeat the sequence forever, The returned collection has infinie size unless the source collection is empty
    /// </summary>
    public static IEnumerable<T> RepeatForever<T>(this IEnumerable<T> source)
    {
        if (source is IList<T> list)
            return list.RepeatForeverList();

        if (source.TryGetNonEnumeratedCount(out var count) && count == 0)
            return Enumerable.Empty<T>();

        return new RepeatForeverQuerier<T>(source);
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

    private sealed class RepeatForeverQuerier<T>(IEnumerable<T> source) : SimpleEnumerationQuerier<T, T>(source)
    {
        public override bool MoveNext()
        {
            const int Initialized = MinPreservedState - 1;

            switch (_state) {
                case -1:
                    _state = Initialized;
                    goto case Initialized;

                // NewEnumerator
                case Initialized:
                    _enumerator = _source.GetEnumerator();
                    goto default;

                // Iterating
                default:
                    if (_enumerator!.MoveNext()) {
                        _current = _enumerator.Current;
                        return true;
                    }
                    else {
                        _enumerator.Dispose();
                        goto case Initialized;
                    }
            }
        }
        protected override EnumerationQuerier<T> Clone() => new RepeatForeverQuerier<T>(_source);
    }
}
