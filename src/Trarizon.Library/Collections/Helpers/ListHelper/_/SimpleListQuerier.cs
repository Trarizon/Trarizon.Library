using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class ListHelper
{
    private abstract class SimpleListQuerier<TList, TIn, TOut>(TList list) : ListQuerier<TOut> where TList : IList<TIn>
    {
        protected readonly TList _list = list;

        public sealed override TOut Current => this[_state];

        public override bool IsReadOnly => _list.IsReadOnly;

        public sealed override bool MoveNext() => ++_state < Count;
    }

    private abstract class SimpleReadOnlyListQuerier<TList, TIn, TOut>(TList list)
        : SimpleListQuerier<TList, TIn, TOut>(list)
        where TList : IList<TIn>
    {
        public sealed override bool IsReadOnly => true;

        public sealed override TOut this[int index]
        {
            get => At(index);
            set => ThrowHelper.ThrowNotSupport(ThrowConstants.QuerierImmutable);
        }

        protected abstract TOut At(int index);
    }
}