using System.Collections;
using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableQuery
{
    public static IEnumerable<T> OfTypeUntil<T, TExcept>(this IEnumerable source) where TExcept : T 
        => new OfTypeUntilQuerier<T, TExcept>(source);


    private sealed class OfTypeUntilQuerier<T, TExcept>(IEnumerable source) : EnumerationQuerier<T> where TExcept : T
    {
        private IEnumerator _enumerator = default!;
        private T _current = default!;
        public override T Current => _current;

        public override bool MoveNext()
        {
            const int ValidCase = MinPreservedState - 1;
            const int End = ValidCase - 1;

            switch (_state) {
                case -1:
                    _enumerator = source.GetEnumerator();
                    _state = ValidCase;
                    goto default;
                case ValidCase:
                    if (!_enumerator.MoveNext()) {
                        _state = End;
                        return false;
                    }

                    if (_enumerator.Current is not T current)
                        goto case ValidCase;

                    if (current is TExcept) {
                        _state = End;
                        return false;
                    }
                    _current = current;
                    return true;
                default: // End
                    return false;
            }
        }
        protected override EnumerationQuerier<T> Clone() => new OfTypeUntilQuerier<T, TExcept>(source);
    }
}
