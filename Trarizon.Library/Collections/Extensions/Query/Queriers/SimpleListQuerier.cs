namespace Trarizon.Library.Collections.Extensions.Query.Queriers;
internal abstract class SimpleListQuerier<TIn, TOut> : ListQuerier<TOut>
{
    protected readonly IList<TIn> _list;

    protected SimpleListQuerier(IList<TIn> list) => _list = list;

    public override TOut Current => this[_state];

    public override bool MoveNext() => ++_state < Count;
}
