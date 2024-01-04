using Trarizon.Library.Collections.Extensions.Helpers.Queriers;
using Trarizon.Library.Wrappers;

namespace Trarizon.Library.Collections.Extensions;
partial class EnumerableQuery
{
    /// <summary>
    /// Combine Where and Select, so you can use intermediate variables
    /// </summary>
    public static IEnumerable<TResult> WhereSelect<T, TResult>(this IEnumerable<T> source, Func<T, Optional<TResult>> whereSelector)
        => new WhereSelectQuerier<T, TResult>(source, whereSelector);


    private sealed class WhereSelectQuerier<T, TResult>(IEnumerable<T> source, Func<T, Optional<TResult>> whereSelector) : SimpleEnumerationQuerier<T, TResult>(source)
    {
        public override bool MoveNext()
        {
            const int Iterating = MinPreservedState - 1;
            const int End = Iterating - 1;

            switch (_state) {
                case -1:
                    _enumerator = _source.GetEnumerator();
                    _state = Iterating;
                    goto default;
                case End:
                    return false;
                default:
                    if (_enumerator!.MoveNext()) {
                        if (whereSelector(_enumerator.Current) is (true, var val)) {
                            _current = val;
                            return true;
                        }
                        else goto default; // Loop 
                    }
                    else {
                        _state = End;
                        return false;
                    }
            }
        }

        protected override EnumerationQuerier<TResult> Clone() => new WhereSelectQuerier<T, TResult>(_source, whereSelector);
    }
}
