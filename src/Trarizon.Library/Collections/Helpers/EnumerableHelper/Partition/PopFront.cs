using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    /// <summary>
    /// Pop specific number of elements, and return the rest
    /// </summary>
    /// <remarks>
    /// Both return value and <paramref name="poppedElements"/> are lazy-initialized,
    /// the values in <paramref name="poppedElements"/> will be cached when enumerated.
    /// </remarks>
    public static IEnumerable<T> PopFront<T>(this IEnumerable<T> source, int count, out IEnumerable<T> poppedElements)
    {
        if (count <= 0) {
            poppedElements = [];
            return source;
        }

        if (source is IList<T> list) {
            var rest = list.PopFrontList(count, out var leadingList);
            poppedElements = leadingList;
            return rest;
        }

        var leadings = new PopFrontCountedQuerier<T>(source, count);
        poppedElements = leadings;
        return leadings.RestCollection;
    }

    /// <summary>
    /// Pop specific number of elements, and return the rest,
    /// popped elements are cached in <paramref name="resultSpan"/>
    /// </summary>
    public static IEnumerable<T> PopFront<T>(this IEnumerable<T> source, Span<T> resultSpan, out int spanLength)
    {
        if (source is IList<T> list) {
            return list.PopFrontList(resultSpan, out spanLength);
        }

        if (resultSpan.Length == 0) {
            spanLength = 0;
            return source;
        }

        var leadings = PopFrontImmediateLeadingCollection<T>.PopInto(source, resultSpan, out spanLength);
        return leadings.RestCollection;
    }

    /// <summary>
    /// Pop first element and return the rest,
    /// first element is <paramref name="firstElement"/>.
    /// If no element in <paramref name="source"/>,
    /// <paramref name="firstElement"/> is <paramref name="defaultValue"/>
    /// </summary>
    public static IEnumerable<T> PopFirst<T>(this IEnumerable<T> source, [NotNullIfNotNull(nameof(defaultValue))] out T? firstElement, T? defaultValue = default!)
    {
        if(source is IList<T> list) 
            return list.PopFirstList(out firstElement, defaultValue);

        firstElement = default;
        var rest = source.PopFront(new Span<T>(ref firstElement!), out int len);
        if (len == 0) {
            firstElement = defaultValue;
            return [];
        }
        return rest;
    }

    /// <summary>
    /// Pop elements until <paramref name="predicate"/> failed.
    /// popped elements are cached in <paramref name="poppedElements"/>
    /// </summary>
    public static IEnumerable<T> PopFrontWhile<T>(this IEnumerable<T> source, Func<T, bool> predicate, out IEnumerable<T> poppedElements)
    {
        if (source.IsFixedSizeEmpty())
            return poppedElements = [];

        var leadings = new PopFrontWhileQuerier<T>(source, predicate);
        poppedElements = leadings;
        return leadings.RestCollection;
    }


    private interface IPopFrontQuerierLeadingCollection<T, TSelf> 
        where TSelf : IPopFrontQuerierLeadingCollection<T, TSelf>
    {
        PopFrontQuerierRestCollection<T, TSelf> RestCollection { get; }

        /// <summary>
        /// Iterate all items in LeadingCollection<>, RestCollection can use <see cref="IEnumerator{T}.Current"/> to get first 
        /// value of rest collection after calling this
        /// </summary>
        /// <returns>
        /// Same as <see cref="IEnumerator.MoveNext"/>, <see langword="false"/> when enumerator meets end
        /// </returns>
        bool MoveUntilRest();

        TSelf CloneWithNewRestCollection();
    }

    private struct PopFrontImmediateLeadingCollection<T> 
        : IPopFrontQuerierLeadingCollection<T, PopFrontImmediateLeadingCollection<T>>
    {
        public PopFrontQuerierRestCollection<T, PopFrontImmediateLeadingCollection<T>> RestCollection { get; private set; }
        /// <summary>
        /// collection count,
        /// Iterated if < 0;
        /// </summary>
        private int _count;
        /// <summary>
        /// What <see cref="MoveUntilRest()"/> will returns after enumerated all
        /// </summary>
        private readonly bool _initState;

        private PopFrontImmediateLeadingCollection(PopFrontQuerierRestCollection<T, PopFrontImmediateLeadingCollection<T>> rest, int count, bool initState)
        {
            RestCollection = rest;
            _count = count;
            _initState = initState;
        }

        public static PopFrontImmediateLeadingCollection<T> PopInto(IEnumerable<T> source, Span<T> buffer, out int actualLength)
        {
            var rest = new PopFrontQuerierRestCollection<T, PopFrontImmediateLeadingCollection<T>>(source);
            bool initState;
            int len = 0;
            rest.EnsureEnumerator();
            while (len < buffer.Length) {
                if (rest.Enumerator.MoveNext()) {
                    buffer[len++] = rest.Enumerator.Current;
                    continue;
                }
                else {
                    initState = false;
                    goto Iterated;
                }
            }
            initState = rest.Enumerator.MoveNext();

        Iterated:
            actualLength = len;
            // Init skipcount as negative, so this time EnumerateAll() will do nothing
            rest.LeadingElements = new(rest, ~len, initState);
            return rest.LeadingElements;
        }

        public bool MoveUntilRest()
        {
            if (_count < 0) {
                return _initState;
            }
            else {
                RestCollection.EnsureEnumerator();
                for (int i = 0; i < _count + 1; i++) {
                    RestCollection.Enumerator.MoveNext();
                }
                _count = ~_count;
                return _initState;
            }
        }

        public readonly PopFrontImmediateLeadingCollection<T> CloneWithNewRestCollection()
        {
            var rest = RestCollection.CloneWithoutLeadingCollection();
            rest.LeadingElements = new(rest, _count < 0 ? ~_count : _count, _initState);
            return rest.LeadingElements;
        }
    }

    private abstract class PopFrontQuerierLeadingCollection<T> 
        : EnumerationQuerier<T>, 
        IPopFrontQuerierLeadingCollection<T, PopFrontQuerierLeadingCollection<T>>
    {
        public PopFrontQuerierRestCollection<T, PopFrontQuerierLeadingCollection<T>> RestCollection { get; private init; }
        protected List<T> _cachedItems = null!;

        protected IEnumerator<T> Enumerator => RestCollection.Enumerator;

        protected PopFrontQuerierLeadingCollection(IEnumerable<T> source)
        {
            RestCollection = new(source) {
                LeadingElements = this
            };
        }

        protected PopFrontQuerierLeadingCollection(PopFrontQuerierLeadingCollection<T> other, bool cloneRestCollection)
        {
            if (cloneRestCollection) {
                RestCollection = other.RestCollection.CloneWithoutLeadingCollection();
                RestCollection.LeadingElements = this;
            }
            else {
                RestCollection = other.RestCollection;
                _cachedItems = other._cachedItems;
            }
        }

        [MemberNotNull(nameof(_cachedItems))]
        protected void EnsureInitialized()
        {
            RestCollection.EnsureEnumerator();
            _cachedItems ??= [];
        }

        public sealed override T Current
        {
            get {
                // Keep sync with MoveNext()
                const int End = MinPreservedState - 1;

                Debug.Assert(_state >= End && _state < _cachedItems.Count);
                if (_state < 0)
                    return default!;

                return _cachedItems[_state];
            }
        }

        public sealed override bool MoveNext()
        {
            // Keep sync in PopFrontWhile
            const int End = MinPreservedState - 1;

            switch (_state) {
                case -1:
                    EnsureInitialized();
                    goto default;
                case End:
                    return false;
                default:
                    _state++;
                    Debug.Assert(_state >= 0);
                    if (TryEnsureAccessible(_state))
                        return true;
                    else {
                        _state = End;
                        return false;
                    }
            }
        }

        public abstract bool MoveUntilRest();

        protected abstract bool TryEnsureAccessible(int index);

        public abstract PopFrontQuerierLeadingCollection<T> CloneWithNewRestCollection();
    }

    private sealed class PopFrontCountedQuerier<T> 
        : PopFrontQuerierLeadingCollection<T>
    {
        private readonly int _maxCount;

        public PopFrontCountedQuerier(IEnumerable<T> source, int maxCount) : base(source)
        {
            _maxCount = maxCount;
        }

        private PopFrontCountedQuerier(PopFrontCountedQuerier<T> other, bool cloneRestCollection) : base(other, cloneRestCollection)
        {
            _maxCount = other._maxCount;
        }

        public override bool MoveUntilRest()
        {
            EnsureInitialized();
            while (_cachedItems.Count < _maxCount) {
                if (!Enumerator.MoveNext())
                    return false;
                _cachedItems.Add(Enumerator.Current);
            }
            // Help rest collection call a move next, so rest collection can use Current 
            // to get first item
            return Enumerator.MoveNext();
        }

        protected override bool TryEnsureAccessible(int index)
        {
            if (index < _cachedItems.Count)
                return true;

            if (index >= _maxCount)
                return false;

            while (_cachedItems.Count <= index) {
                if (!Enumerator.MoveNext())
                    return false;

                _cachedItems.Add(Enumerator.Current);
            }
            return true;
        }

        protected override EnumerationQuerier<T> Clone() => new PopFrontCountedQuerier<T>(this, false);

        public override PopFrontQuerierLeadingCollection<T> CloneWithNewRestCollection() => new PopFrontCountedQuerier<T>(this, true);
    }

    private sealed class PopFrontWhileQuerier<T>
        : PopFrontQuerierLeadingCollection<T>
    {
        private readonly Func<T, bool> _predicate;
        private bool _sourceFullEnumerated;

        public PopFrontWhileQuerier(IEnumerable<T> source, Func<T, bool> predicate) : base(source)
        {
            _predicate = predicate;
        }

        private PopFrontWhileQuerier(PopFrontWhileQuerier<T> other, bool cloneRestCollection) : base(other, cloneRestCollection)
        {
            _predicate = other._predicate;
        }

        public override bool MoveUntilRest()
        {
            const int End = MinPreservedState - 1;
            if (_state == End) {
                return !_sourceFullEnumerated;
            }
            EnsureInitialized();
            while (Enumerator.MoveNext()) {
                var cur = Enumerator.Current;
                if (_predicate(cur)) {
                    _cachedItems.Add(cur);
                    continue;
                }
                return true;
            }
            return false;
        }

        protected override bool TryEnsureAccessible(int index)
        {
            if (index < _cachedItems.Count) {
                return true;
            }

            while (_cachedItems.Count <= index) {
                if (!Enumerator.MoveNext()) {
                    _sourceFullEnumerated = true;
                }
                else {
                    var cur = Enumerator.Current;
                    if (_predicate(cur)) {
                        _cachedItems.Add(cur);
                        continue;
                    }
                }
                return false;
            }
            return true;
        }

        protected override EnumerationQuerier<T> Clone() => new PopFrontWhileQuerier<T>(this, false);

        public override PopFrontQuerierLeadingCollection<T> CloneWithNewRestCollection() => new PopFrontWhileQuerier<T>(this, true);
    }

    private sealed class PopFrontQuerierRestCollection<T, TLeadingCollection> 
        : EnumerationQuerier<T>
        where TLeadingCollection : IPopFrontQuerierLeadingCollection<T, TLeadingCollection>
    {
        private readonly IEnumerable<T> _source;
        public IEnumerator<T> Enumerator = default!;
        private T _current = default!;

        public TLeadingCollection LeadingElements
        {
            get;
            [FriendAccess(typeof(PopFrontQuerierLeadingCollection<>), typeof(PopFrontImmediateLeadingCollection<>))]
            set;
        }

        internal PopFrontQuerierRestCollection(IEnumerable<T> source)
        {
            _source = source;
            LeadingElements = default!;
        }

        public override T Current => _current;

        [MemberNotNull(nameof(Enumerator))]
        public void EnsureEnumerator()
            => Enumerator ??= _source.GetEnumerator();

        public override bool MoveNext()
        {
            const int Iterate = MinPreservedState - 1;
            const int End = Iterate - 1;

            switch (_state) {
                case -1:
                    if (LeadingElements.MoveUntilRest()) {
                        _current = Enumerator.Current;
                        _state = Iterate;
                        return true;
                    }
                    else {
                        _state = End;
                        return false;
                    }
                case Iterate:
                    if (Enumerator.MoveNext()) {
                        _current = Enumerator.Current;
                        return true;
                    }
                    else {
                        _state = End;
                        return false;
                    }
                case End:
                default:
                    return false;
            }
        }

        protected override EnumerationQuerier<T> Clone() => LeadingElements.CloneWithNewRestCollection().RestCollection;

        internal PopFrontQuerierRestCollection<T, TLeadingCollection> CloneWithoutLeadingCollection() => new(_source);
    }
}
