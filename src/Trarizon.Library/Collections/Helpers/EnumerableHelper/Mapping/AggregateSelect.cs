﻿using System.Numerics;
using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    /// <summary>
    /// Aggregate, and returns all values in process
    /// </summary>
    public static IEnumerable<T> AggregateSelect<T>(this IEnumerable<T> source, Func<T, T, T> func) where T : INumber<T>
        => source.AggregateSelect(T.Zero, func);

    /// <summary>
    /// Aggregate, and returns all values in process
    /// </summary>
    public static IEnumerable<TAccumulate> AggregateSelect<T, TAccumulate>(this IEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
        => source.AggregateSelect(seed, func, x => x);

    /// <summary>
    /// Aggregate, and returns all values in process
    /// </summary>
    public static IEnumerable<TResult> AggregateSelect<T, TAccumulate, TResult>(this IEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
    {
        if (source.IsFixedSizeEmpty())
            return [];

        return new AggregateSelectQuerier<T, TAccumulate, TResult>(source, seed, func, resultSelector);
    }


    private sealed class AggregateSelectQuerier<T, TAcc, TResult>(
        IEnumerable<T> source,
        TAcc seed,
        Func<TAcc, T, TAcc> func,
        Func<TAcc, TResult> resultSelector)
        : SimpleEnumerationQuerier<T, TResult>(source)
    {
        private TAcc? _accumulate;

        public override bool MoveNext()
        {
            if (_state == -1) {
                _accumulate = seed;
                _enumerator = _source.GetEnumerator();
                _state = 0;
            }

            if (_enumerator!.MoveNext()) {
                _accumulate = func(_accumulate!, _enumerator.Current);
                _current = resultSelector(_accumulate);
                return true;
            }
            return false;
        }

        protected override EnumerationQuerier<TResult> Clone() => new AggregateSelectQuerier<T, TAcc, TResult>(_source, seed, func, resultSelector);
    }
}
