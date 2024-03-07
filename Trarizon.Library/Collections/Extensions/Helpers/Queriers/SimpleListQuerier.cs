﻿namespace Trarizon.Library.Collections.Extensions.Helpers.Queriers;
internal abstract class SimpleListQuerier<TList, TIn, TOut>(TList list) : ListQuerier<TOut> where TList : IReadOnlyList<TIn>
{
    protected readonly TList _list = list;

    public sealed override TOut Current => this[_state];

    public sealed override bool MoveNext() => ++_state < Count;
}
