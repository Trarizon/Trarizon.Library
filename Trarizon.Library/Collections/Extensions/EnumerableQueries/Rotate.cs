using Trarizon.Library.Collections.Extensions.Helpers.Queriers;

namespace Trarizon.Library.Collections.Extensions;
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

        return new RotateQuerier<T>(source, splitPosition);
    }


    private sealed class RotateQuerier<T>(IEnumerable<T> source, int splitPosition) : SimpleEnumerationQuerier<T, T>(source)
    {
        private readonly T[] _firstPart = new T[splitPosition];
        private int _length;

        public override bool MoveNext()
        {
            const int IterateSecondPart = MinPreservedState - 1;

            switch (_state) {
                case -1:
                    _enumerator = _source.GetEnumerator();
                    _state = 0;
                    for (_length = 0; _length < _firstPart.Length; _length++) {
                        // splitPosition > _source.Count, iterate cache.
                        if (!_enumerator.MoveNext()) {
                            _length++;
                            // _state as index
                            _state = -1;
                            goto default;
                        }

                        // Cache
                        _firstPart[_length] = _enumerator.Current;
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
                    if (++_state >= _firstPart.Length)
                        return false;

                    _current = _firstPart[_state];
                    return true;
            }
        }
        protected override EnumerationQuerier<T> Clone() => new RotateQuerier<T>(_source, _firstPart.Length);
    }

}
