using Trarizon.Library.Collections.Extensions.Query.Queriers;

namespace Trarizon.Library.Collections.Extensions.Query;
partial class EnumerableQuery
{
    /// <summary>
    /// return an element and its next element each time.
    /// </summary>
    public static IEnumerable<(T, T)> Adjacent<T>(this IEnumerable<T> source) => new AdjacentQuerier<T>(source);


    private sealed class AdjacentQuerier<T>(IEnumerable<T> source) : SimpleEnumerationQuerier<T, (T Prev, T Curr)>(source)
    {
        public override bool MoveNext()
        {
            const int Iterating = MinPreservedState - 1;
            const int NoMoreElement = Iterating - 1;

            switch (_state) {
                case -1:
                    _enumerator = _source.GetEnumerator();

                    if (_enumerator.MoveNext()) {
                        _current.Prev = _enumerator.Current;
                        if (_enumerator.MoveNext()) {
                            _current.Curr = _enumerator.Current;
                            _state = Iterating;
                            return true;
                        }
                    }
                    _state = NoMoreElement;
                    return false;
                case Iterating:
                    if (_enumerator!.MoveNext()) {
                        _current.Prev = _current.Curr;
                        _current.Curr = _enumerator.Current;
                        return true;
                    }
                    _state = NoMoreElement;
                    return false;
                case NoMoreElement:
                default:
                    return false;
            }
        }

        protected override EnumerationQuerier<(T, T)> Clone() => new AdjacentQuerier<T>(_source);
    }
}
