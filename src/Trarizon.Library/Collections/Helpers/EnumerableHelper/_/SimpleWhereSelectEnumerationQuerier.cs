namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    private abstract class SimpleWhereSelectEnumerationQuerier<TIn, TOut>(IEnumerable<TIn> source)
        : SimpleEnumerationQuerier<TIn, TOut>(source)
    {
        public sealed override bool MoveNext()
        {
            const int Iterate = MinPreservedState - 1;
            const int End = Iterate - 1;

            switch (_state) {
                case -1:
                    InitializeEnumerator();
                    _state = Iterate;
                    goto case Iterate;
                case Iterate:
                    if (_enumerator.MoveNext()) {
                        var current = _enumerator.Current;

                        if (TrySetValue(current)) {
                            return true;
                        }
                        else {
                            goto case Iterate;
                        }
                    }
                    _state = End;
                    goto default;
                case End:
                default:
                    return false;
            }
        }

        protected abstract bool TrySetValue(TIn inVal);
    }
}
