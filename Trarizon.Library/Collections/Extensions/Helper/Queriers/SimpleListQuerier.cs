namespace Trarizon.Library.Collections.Extensions.Helper.Queriers;
internal abstract class SimpleListQuerier<TList, TIn, TOut> : ListQuerier<TOut> where TList : IReadOnlyList<TIn>
{
    protected readonly TList _list;

    protected SimpleListQuerier(TList list) => _list = list;

    public override TOut Current => this[_state];

    public override bool MoveNext() => ++_state < Count;
}
