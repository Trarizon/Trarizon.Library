using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class ListQuery
{
    private abstract class SimpleListQuerier<TList, TIn, TOut>(TList list) : ListQuerier<TOut> where TList : IList<TIn>
    {
        protected readonly TList _list = list;

        public sealed override TOut Current => this[_state];

        public sealed override bool MoveNext() => ++_state < Count;
    }
}