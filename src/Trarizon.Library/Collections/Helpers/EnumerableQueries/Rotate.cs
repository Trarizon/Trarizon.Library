using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableQuery
{
    /// <summary>
    /// Swap two parts of sequence
    /// </summary>
    public static IEnumerable<T> Rotate<T>(this IEnumerable<T> source, int splitPosition)
    {
        if (splitPosition == 0)
            return source;

        if (source is IList<T> list)
            return list.RotateList(splitPosition);

        if (source.TryGetNonEnumeratedCount(out var count) && splitPosition >= count)
            return source;

        return new RotateQuerier<T>(source, splitPosition);
    }


    private sealed class RotateQuerier<T>(IEnumerable<T> source, int splitPosition) : SimpleEnumerationQuerier<T, T>(source)
    {
        private AllocOptList<T> _firstPart;

        public override bool MoveNext()
        {
            const int IterateSecondPart = MinPreservedState - 1;

            switch (_state) {
                case -1:
                    _enumerator = _source.GetEnumerator();
                    _state = 0;
                    // Initialize first part cache
                    if (_source.TryGetNonEnumeratedCount(out _))
                        _firstPart = new(splitPosition);
                    else
                        _firstPart = [];

                    for (int length = 0; length < splitPosition; length++) {
                        // splitPosition > _source.Count, iterate cache.
                        if (_enumerator.MoveNext()) {
                            // Cache
                            _firstPart.Add(_enumerator.Current);
                            continue;
                        }
                        else {
                            // Do not has 2nd part
                            // _state as index
                            _state = -1;
                            goto default;
                        }
                    }
                    _state = IterateSecondPart;
                    goto case IterateSecondPart;

                case IterateSecondPart:
                    if (_enumerator!.MoveNext()) {
                        _current = _enumerator.Current;
                        return true;
                    }
                    else {
                        // _state as index
                        _state = -1;
                        goto default;
                    }

                // Iterate first part
                default:
                    if (++_state >= _firstPart.Count)
                        return false;

                    _current = _firstPart[_state];
                    return true;
            }
        }
        protected override EnumerationQuerier<T> Clone() => new RotateQuerier<T>(_source, splitPosition);
    }

}
