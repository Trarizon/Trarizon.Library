using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Helpers.Utilities.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableQuery
{
    /// <summary>
    /// Pop specific number of elements, and return the rest,
    /// popped elements are cached in <paramref name="leadingElements"/>
    /// </summary>
    //public static IEnumerable<T> PopFront<T>(this IEnumerable<T> source, int count, out IReadOnlyList<T> leadingElements)
    //{
    //    if (count <= 0) {
    //        leadingElements = Array.Empty<T>();
    //        return source;
    //    }

    //    if (source is IList<T> list) {
    //        return list.PopFrontList(count, out leadingElements);
    //    }

    //    var enumerator = source.GetEnumerator();
    //    var buffer = new AllocOptList<T>(count);
    //    while (buffer.Count < count && enumerator.MoveNext()) {
    //        buffer.Add(enumerator.Current);
    //    }

    //    if (buffer.Count < count) {
    //        leadingElements = buffer.GetUnderlyingArray().TakeROList(..buffer.Count);
    //        return Enumerable.Empty<T>();
    //    }
    //    else {
    //        leadingElements = buffer.GetUnderlyingArray();
    //        return new PopFrontQuerier<T>(source, enumerator, count);
    //    }
    //}

    public static IEnumerable<T> PopFront<T>(this IEnumerable<T> source, Span<T> resultSpan, out int spanLength)
    {
        if (source is IList<T> list) {
            return list.PopFrontList(resultSpan, out spanLength);
        }

        var rest = PopFrontImmediateLeadingCollection<T>.PopAndGetRest(source, ref resultSpan);
        spanLength = resultSpan.Length;
        return rest;
    }

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
            poppedElements = Enumerable.Empty<T>();
            return source;
        }

        if (source is IList<T> list) {
            var rest = list.PopFrontList(count, out var leadingList);
            poppedElements = leadingList;
            return rest;
        }

        var leadings = new PopCountedFrontQuerier<T>(source, count);
        poppedElements = leadings;
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
        firstElement = default;
        var rest = source.PopFront(new Span<T>(ref firstElement!), out int len);
        if (len == 0) {
            firstElement = defaultValue;
            return Enumerable.Empty<T>();
        }
        return rest;
    }

    /// <summary>
    /// Pop elements until <paramref name="predicate"/> failed.
    /// popped elements are cached in <paramref name="poppedElements"/>
    /// </summary>
    public static IEnumerable<T> PopFrontWhile<T>(this IEnumerable<T> source, out IEnumerable<T> poppedElements, Func<T, bool> predicate)
    {
        if (source is IList<T> list) {
            var restList = list.PopFrontWhileList(out var leadingList, predicate);
            poppedElements = leadingList;
            return restList;
        }

        var leadings = new PopFrontWhileQuerier<T>(source, predicate);
        poppedElements = leadings;
        return leadings.RestCollection;
    }

    private interface IPopFrontQuerierLeadingCollection<T, TSelf> where TSelf : IPopFrontQuerierLeadingCollection<T, TSelf>
    {
        void EnumerateAll();

        TSelf CloneWith(PopFrontQuerierRestCollection<T, TSelf> rest);
    }

    private readonly struct PopFrontImmediateLeadingCollection<T> : IPopFrontQuerierLeadingCollection<T, PopFrontImmediateLeadingCollection<T>>
    {
        public readonly PopFrontQuerierRestCollection<T, PopFrontImmediateLeadingCollection<T>> RestCollection;
        private readonly int _skipCount;

        private PopFrontImmediateLeadingCollection(PopFrontQuerierRestCollection<T, PopFrontImmediateLeadingCollection<T>> rest, int skipCount)
        {
            RestCollection = rest;
            _skipCount = skipCount;
        }

        public static IEnumerable<T> PopAndGetRest(IEnumerable<T> source, ref Span<T> buffer)
        {
            var rest = new PopFrontQuerierRestCollection<T, PopFrontImmediateLeadingCollection<T>>(source, default, false);
            int len = 0;
            rest.EnsureEnumerator();
            while (len < buffer.Length && rest.Enumerator.MoveNext()) {
                buffer[len++] = rest.Enumerator.Current;
            }
            if (len < buffer.Length)
                buffer = buffer[..len];

            // Init skipcount as negative, so this time EnumerateAll() will do nothing
            rest.LeadingElements = new(rest, -len);
            return rest;
        }

        public readonly PopFrontImmediateLeadingCollection<T> CloneWith(PopFrontQuerierRestCollection<T, PopFrontImmediateLeadingCollection<T>> rest) => new(rest, int.Abs(_skipCount));

        public void EnumerateAll()
        {
            RestCollection.EnsureEnumerator();
            for (int i = 0; i < _skipCount; i++) {
                RestCollection.Enumerator.MoveNext();
            }
        }
    }

    private abstract class PopFrontQuerierLeadingCollection<T> : EnumerationQuerier<T>, IPopFrontQuerierLeadingCollection<T, PopFrontQuerierLeadingCollection<T>>
    {
        public readonly PopFrontQuerierRestCollection<T, PopFrontQuerierLeadingCollection<T>> RestCollection;
        protected List<T> _cachedItems = default!;

        protected IEnumerator<T> Enumerator => RestCollection.Enumerator;

        protected PopFrontQuerierLeadingCollection(IEnumerable<T> source)
        {
            RestCollection = new(source, this, false);
        }

        protected PopFrontQuerierLeadingCollection(PopFrontQuerierRestCollection<T, PopFrontQuerierLeadingCollection<T>> rest)
        {
            RestCollection = rest;
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

        public void EnumerateAll()
        {
            while (MoveNext()) { }
        }

        public sealed override bool MoveNext()
        {
            const int End = MinPreservedState - 1;

            switch (_state) {
                case -1:
                    RestCollection.EnsureEnumerator();
                    _cachedItems = [];
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

        protected abstract bool TryEnsureAccessible(int index);

        [FriendAccess(typeof(PopFrontQuerierRestCollection<,>))]
        public abstract PopFrontQuerierLeadingCollection<T> CloneWith(PopFrontQuerierRestCollection<T, PopFrontQuerierLeadingCollection<T>> rest);
    }

    private sealed class PopCountedFrontQuerier<T> : PopFrontQuerierLeadingCollection<T>
    {
        private readonly int _maxCount;

        public PopCountedFrontQuerier(IEnumerable<T> source, int maxCount) : base(source)
        {
            _maxCount = maxCount;
        }

        private PopCountedFrontQuerier(PopFrontQuerierRestCollection<T, PopFrontQuerierLeadingCollection<T>> rest, int maxCount)
            : base(rest)
        {
            _maxCount = maxCount;
        }

        protected override EnumerationQuerier<T> Clone()
            => new PopCountedFrontQuerier<T>(RestCollection, _maxCount);

        public override PopFrontQuerierLeadingCollection<T> CloneWith(PopFrontQuerierRestCollection<T, PopFrontQuerierLeadingCollection<T>> rest)
            => new PopCountedFrontQuerier<T>(rest, _maxCount);

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
    }

    private sealed class PopFrontWhileQuerier<T> : PopFrontQuerierLeadingCollection<T>
    {
        private readonly Func<T, bool> _predicate;

        public PopFrontWhileQuerier(IEnumerable<T> source, Func<T, bool> predicate) : base(source)
        {
            _predicate = predicate;
        }

        private PopFrontWhileQuerier(PopFrontQuerierRestCollection<T, PopFrontQuerierLeadingCollection<T>> rest, Func<T, bool> predicate)
            : base(rest)
        {
            _predicate = predicate;
        }

        protected override EnumerationQuerier<T> Clone()
            => new PopFrontWhileQuerier<T>(RestCollection, _predicate);

        public override PopFrontQuerierLeadingCollection<T> CloneWith(PopFrontQuerierRestCollection<T, PopFrontQuerierLeadingCollection<T>> rest)
            => new PopFrontWhileQuerier<T>(rest, _predicate);

        protected override bool TryEnsureAccessible(int index)
        {
            if (index < _cachedItems.Count) {
                return true;
            }

            while (_cachedItems.Count <= index) {
                if (Enumerator.MoveNext()) {
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
    }

    private sealed class PopFrontQuerierRestCollection<T, TLeadingCollection> : EnumerationQuerier<T> where TLeadingCollection : IPopFrontQuerierLeadingCollection<T, TLeadingCollection>
    {
        private readonly IEnumerable<T> _source;
        public IEnumerator<T> Enumerator = default!;
        private T _current = default!;

        public TLeadingCollection LeadingElements
        {
            get;
            // For reason of copy pass of value type, we need to public
            // this set to structs
            [FriendAccess(typeof(PopFrontImmediateLeadingCollection<>))]
            set;
        }

        [FriendAccess(typeof(PopFrontQuerierLeadingCollection<>), typeof(PopFrontImmediateLeadingCollection<>))]
        internal PopFrontQuerierRestCollection(IEnumerable<T> source, TLeadingCollection leadings, bool cloneLeadings)
        {
            _source = source;
            LeadingElements = cloneLeadings ? leadings.CloneWith(this) : leadings;
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
                    LeadingElements.EnumerateAll();
                    _state = Iterate;
                    goto case Iterate;
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

        protected override EnumerationQuerier<T> Clone() => new PopFrontQuerierRestCollection<T, TLeadingCollection>(_source, LeadingElements, true);
    }
}
