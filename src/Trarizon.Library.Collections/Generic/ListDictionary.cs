using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.Helpers;

#if NETSTANDARD
#pragma warning disable CS8767
#endif

namespace Trarizon.Library.Collections.Generic;

[CollectionBuilder(typeof(CollectionBuilders), nameof(CollectionBuilders.CreateListDictionary))]
public class ListDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue> where TKey : notnull
{
    private (TKey Key, TValue Value)[] _pairs;
    private int _count;
    private IEqualityComparer<TKey>? _comparer;
    private int _version;

    public ListDictionary(int capacity, IEqualityComparer<TKey>? comparer = null)
    {
        Throws.ThrowIfNegative(capacity);
        _pairs = capacity == 0 ? [] : new (TKey, TValue)[capacity];
        _comparer = comparer;
    }

    public ListDictionary(IEqualityComparer<TKey>? comparer = null)
    {
        _pairs = [];
        _comparer = comparer;
    }

    public TValue this[TKey key]
    {
        get {
            ref var item = ref FindRef(key);
            if (Unsafe.IsNullRef(ref item)) {
                Throws.KeyNotFound(key, nameof(ListDictionary<,>));
                return default;
            }

            return item.Value;
        }
        set {
            ref var item = ref FindRef(key);
            if (Unsafe.IsNullRef(ref item)) {
                AddInternal(key, value);
            }
            else {
                item.Value = value;
            }
            _version++;
        }
    }

    public KeyCollection Keys => new(this);

    public ValueCollection Values => new(this);

    public int Count => _count;

    public int Capacity => _pairs.Length;

    public void Add(TKey key, TValue value)
    {
        if (!TryAdd(key, value))
            Throws.KeyAlreadyExists(key, nameof(ListDictionary<,>));
        return;
    }

    public bool TryAdd(TKey key, TValue value)
    {
        ref var val = ref FindRef(key);
        if (!Unsafe.IsNullRef(ref val)) {
            return false;
        }
        AddInternal(key, value);
        return true;
    }

    public void Clear()
    {
        ArrayGrowHelper.FreeIfReferenceOrContainsReferences(_pairs.AsSpan(0, _count));
        _count = 0;
        _version++;
    }

    public bool ContainsKey(TKey key) => !Unsafe.IsNullRef(ref FindRef(key));

    public bool Remove(TKey key)
    {
        ref var item = ref FindRef(key);
        if (Unsafe.IsNullRef(ref item))
            return false;

        RemoveRef(in item);
        _version++;
        return true;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        ref var item = ref FindRef(key);
        if (Unsafe.IsNullRef(ref item)) {
            value = default;
            return false;
        }
        value = item.Value;
        return true;
    }

    private void AddInternal(TKey key, TValue value)
    {
        if (_count == _pairs.Length) {
            ArrayGrowHelper.Grow(ref _pairs, _count + 1, _count);
        }
        _pairs[_count++] = (key, value);
        _version++;
    }

    private ref (TKey Key, TValue Value) FindRef(TKey key)
    {
        if (typeof(TKey).IsValueType && _comparer is null) {
            foreach (ref var pair in _pairs.AsSpan(_count)) {
                if (EqualityComparer<TKey>.Default.Equals(pair.Key, key))
                    return ref pair;
            }
        }
        else {
            _comparer ??= EqualityComparer<TKey>.Default;
            foreach (ref var pair in _pairs.AsSpan(_count)) {
                if (_comparer.Equals(pair.Key, key))
                    return ref pair;
            }
        }
        return ref Unsafe.NullRef<(TKey, TValue)>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item">Must be item in <see cref="_pairs"/></param>
    private void RemoveRef(ref readonly (TKey, TValue) item)
    {
        var index = _pairs.AsSpan().OffsetOf(in item);
        ArrayGrowHelper.ShiftLeftForRemoveAndFree(_pairs, _count, index, 1);
        _count--;
    }

    public void EnsureCapacty(int capacity)
    {
        if (capacity <= _pairs.Length)
            return;
        ArrayGrowHelper.Grow(ref _pairs, capacity, _count);
    }

    public Enumerator GetEnumerator() => new(this);

    #region Interface

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

#if NETSTANDARD2_0

    bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value) => TryGetValue(key, out value!);
    bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value) => TryGetValue(key, out value!);

#endif

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        ref var findItem = ref FindRef(item.Key);
        if (Unsafe.IsNullRef(ref findItem))
            return false;

        return EqualityComparer<TValue>.Default.Equals(findItem.Value, item.Value);
    }
    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        Throws.ThrowIfNegative(arrayIndex);
        Throws.ThrowIfLessThan(array.Length, arrayIndex + Count);

        if (Count == 0)
            return;

        foreach (var (k, v) in _pairs) {
            array[arrayIndex++] = new KeyValuePair<TKey, TValue>(k, v);
        }
    }
    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        ref var findItem = ref FindRef(item.Key);
        if (Unsafe.IsNullRef(ref findItem))
            return false;
        if (!EqualityComparer<TValue>.Default.Equals(findItem.Value, item.Value))
            return false;

        RemoveRef(in findItem);
        _version++;
        return true;
    }
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private readonly int _version;
        private readonly ListDictionary<TKey, TValue> _dict;
        private int _index;
        private KeyValuePair<TKey, TValue> _current;

        internal Enumerator(ListDictionary<TKey, TValue> dictionary)
        {
            _dict = dictionary;
            _version = _dict._version;
            _index = 0;
        }

        public readonly KeyValuePair<TKey, TValue> Current => _current;

        readonly object IEnumerator.Current => Current;

        public readonly void Dispose() { }
        public bool MoveNext()
        {
            CheckVersion();
            if (_index < 0)
                return false;

            if (_index < _dict.Count) {
                var (key, value) = _dict._pairs[_index];
                _current = new KeyValuePair<TKey, TValue>(key, value);
                _index++;
                return true;
            }
            else {
                _index = -1;
                _current = default;
                return false;
            }
        }

        public void Reset()
        {
            CheckVersion();
            _index = 0;
        }

        private readonly void CheckVersion()
        {
            if (_version != _dict._version)
                Throws.CollectionModifiedDuringEnumeration();
        }
    }

    public readonly struct KeyCollection : ICollection<TKey>
    {
        private readonly ListDictionary<TKey, TValue> _dict;

        internal KeyCollection(ListDictionary<TKey, TValue> dictionary)
            => _dict = dictionary;

        public int Count => _dict.Count;

        public bool IsReadOnly => true;

        public bool Contains(TKey item) => _dict.ContainsKey(item);

        public void CopyTo(TKey[] array, int arrayIndex)
        {
            Throws.ThrowIfNegative(arrayIndex);
            Throws.ThrowIfLessThan(array.Length, arrayIndex + Count);

            if (Count == 0)
                return;

            foreach (var (k, _) in _dict._pairs) {
                array[arrayIndex++] = k;
            }
        }

        public Enumerator GetEnumerator() => new(_dict);

        void ICollection<TKey>.Add(TKey item) => Throws.ThrowNotSupport();
        void ICollection<TKey>.Clear() => Throws.ThrowNotSupport();
        IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => GetEnumerator();
        bool ICollection<TKey>.Remove(TKey item) => false;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<TKey>
        {
            private ListDictionary<TKey, TValue>.Enumerator _enumerator;

            //private AllocOptList<(TKey, TValue)>.Enumerator _enumerator;

            internal Enumerator(ListDictionary<TKey, TValue> dict)
                => _enumerator = dict.GetEnumerator();

            public readonly TKey Current => _enumerator.Current.Key;

            readonly object IEnumerator.Current => Current;

            public readonly void Dispose() { }
            public bool MoveNext() => _enumerator.MoveNext();
            void IEnumerator.Reset() => _enumerator.Reset();
        }
    }

    public readonly struct ValueCollection : ICollection<TValue>
    {
        private readonly ListDictionary<TKey, TValue> _dict;

        internal ValueCollection(ListDictionary<TKey, TValue> dictionary)
            => _dict = dictionary;

        public int Count => _dict.Count;

        public bool IsReadOnly => true;

        public bool Contains(TValue item)
        {
            foreach (var (_, v) in _dict._pairs) {
                if (EqualityComparer<TValue>.Default.Equals(v, item))
                    return true;
            }
            return false;
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            Throws.ThrowIfNegative(arrayIndex);
            Throws.ThrowIfLessThan(array.Length, arrayIndex + Count);

            if (Count == 0)
                return;

            foreach (var (_, v) in _dict._pairs) {
                array[arrayIndex++] = v;
            }
        }

        public Enumerator GetEnumerator() => new(_dict);

        void ICollection<TValue>.Add(TValue item) => Throws.ThrowNotSupport();
        void ICollection<TValue>.Clear() => Throws.ThrowNotSupport();
        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => GetEnumerator();
        bool ICollection<TValue>.Remove(TValue item) => false;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<TValue>
        {
            private ListDictionary<TKey, TValue>.Enumerator _enumerator;

            internal Enumerator(ListDictionary<TKey, TValue> dict)
                => _enumerator = dict.GetEnumerator();

            public readonly TValue Current => _enumerator.Current.Value;

            readonly object? IEnumerator.Current => Current;

            public readonly void Dispose() { }
            public bool MoveNext() => _enumerator.MoveNext();
            void IEnumerator.Reset() => _enumerator.Reset();
        }
    }
}
