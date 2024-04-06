using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.Helpers;
partial class ListHelper
{
    private abstract class SimpleIndexMapListQuerier<TList, T>(TList list) : SimpleListQuerier<TList, T, T>(list) where TList : IList<T>
    {
        public sealed override T this[int index]
        {
            get => _list[ValdiateAndSelectIndex(index)];
            set => _list[ValdiateAndSelectIndex(index)] = value;
        }

        protected abstract int ValdiateAndSelectIndex(int index);

        protected void ValidateOutOfRange(int index, [CallerArgumentExpression(nameof(index))] string? paramName = null) 
            => ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count, paramName);
        protected static void ValidateNegative(int index, [CallerArgumentExpression(nameof(index))] string? paramName = null) 
            => ArgumentOutOfRangeException.ThrowIfNegative(index, paramName);
    }
}