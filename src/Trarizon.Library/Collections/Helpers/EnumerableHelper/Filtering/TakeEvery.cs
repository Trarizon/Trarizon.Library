using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    public static IEnumerable<T> TakeEvery<T>(this IEnumerable<T> source, int interval)
    {
        if (interval <= 1)
            return source;

        if (source.IsFixedSizeEmpty())
            return [];

        if (source is IList<T> list)
            return list.TakeEveryList(interval);

        return new TakeEveryQuerier<T>(source, interval);
    }


    private sealed class TakeEveryQuerier<T>(IEnumerable<T> source, int interval) : SimpleEnumerationQuerier<T, T>(source)
    {
        public override bool MoveNext()
        {
            const int Iterate = MinPreservedState - 1;
            const int End = Iterate - 1;

            switch (_state) {
                case -1:
                    InitializeEnumerator();
                    if (_enumerator.MoveNext()) {
                        _current = _enumerator.Current;
                        _state = Iterate;
                        return true;
                    }
                    else {
                        _state = End;
                        return false;
                    }
                case Iterate:
                    for (int i = 0; i < interval; i++) {
                        if (!_enumerator.MoveNext()) {
                            _state = End;
                            return false;
                        }
                    }
                    _current = _enumerator.Current;
                    return true;
                default:
                    return false;
            }
        }

        protected override EnumerationQuerier<T> Clone() => new TakeEveryQuerier<T>(_source, interval);
    }
}
