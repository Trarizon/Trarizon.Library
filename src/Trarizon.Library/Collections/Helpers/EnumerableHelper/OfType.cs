using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections.Helpers.Queriers;
using Trarizon.Library.Wrappers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    public static IEnumerable<T> OfNotNull<T>(this IEnumerable<T?> source) where T : class
    {
        if (source.IsFixedSizeEmpty())
            return [];

        return new OfReferenceTypeNotNullQuerier<T>(source);
    }

    public static IEnumerable<T> OfNotNull<T>(this IEnumerable<T?> source) where T : struct
    {
        if (source.IsFixedSizeEmpty())
            return [];

        return new OfValueTypeNotNullQuerier<T>(source);
    }

    public static IEnumerable<T> OfNotNone<T>(this IEnumerable<Optional<T>> source)
    {
        if (source.IsFixedSizeEmpty())
            return [];

        return new OfOptionalNotNoneQuerier<T, Optional<T>>(source);
    }

    public static IEnumerable<T> OfTypeUntil<T, TExcept>(this IEnumerable source) where TExcept : T
    {
        if (source.IsFixedSizeEmpty())
            return [];

        return new OfTypeUntilQuerier<T, TExcept>(source);
    }


    private sealed class OfReferenceTypeNotNullQuerier<T>(IEnumerable<T?> source) : SimpleWhereSelectEnumerationQuerier<T?, T>(source) where T : class
    {
        protected override EnumerationQuerier<T> Clone() => new OfReferenceTypeNotNullQuerier<T>(_source);

        protected override bool TrySelect(T? inVal, [MaybeNullWhen(false)] out T outVal)
        {
            outVal = inVal;
            return inVal is not null;
        }
    }

    private sealed class OfValueTypeNotNullQuerier<T>(IEnumerable<T?> source) : SimpleWhereSelectEnumerationQuerier<T?, T>(source) where T : struct
    {
        protected override EnumerationQuerier<T> Clone() => new OfValueTypeNotNullQuerier<T>(_source);
        protected override bool TrySelect(T? inVal, [MaybeNullWhen(false)] out T outVal)
        {
            outVal = inVal.GetValueOrDefault();
            return inVal is not null;
        }
    }

    private sealed class OfOptionalNotNoneQuerier<T, TOptional>(IEnumerable<TOptional> source) : SimpleWhereSelectEnumerationQuerier<TOptional, T>(source) where TOptional : IOptional<T>
    {
        protected override EnumerationQuerier<T> Clone() => new OfOptionalNotNoneQuerier<T, TOptional>(_source);
        protected override bool TrySelect(TOptional inVal, [MaybeNullWhen(false)] out T outVal)
        {
            return inVal.TryGetValue(out outVal);
        }
    }

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
