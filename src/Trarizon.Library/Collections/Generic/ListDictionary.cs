using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections.Generic;
public class ListDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue> where TKey : notnull
{
    private readonly List<(TKey Key, TValue Value)> _pairs;
    private IEqualityComparer<TKey>? _comparer;

    public ListDictionary(int capacity, IEqualityComparer<TKey>? comparer = null)
    {
        _pairs = new(capacity);
        _comparer = comparer;
    }

    public ListDictionary(IEqualityComparer<TKey>? comparer = null)
    {
        _pairs = new();
        _comparer = comparer;
    }

    public TValue this[TKey key]
    {
        get {
            ref readonly var item = ref FindRef(key);
            if (Unsafe.IsNullRef(in item))
                throw new KeyNotFoundException($"Cannot find key '{key}' in collection");

            return item.Value;
        }
        set {
            FindRefOrAddDefault(key, out var _) = value;
        }
    }

    public KeyCollection Keys => new(this);

    public ValueCollection Values => new(this);

    public int Count => _pairs.Count;

    public int Capacity => _pairs.Capacity;

    public void Add(TKey key, TValue value)
    {
        if (!TryAdd(key, value))
            ThrowHelper.ThrowArgumentException(nameof(key), $"Key duplicated.");
        return;
    }

    public bool TryAdd(TKey key, TValue value)
    {
        ref var val = ref FindRefOrAddDefault(key, out var exist);
        if (exist)
            return false;

        val = value;
        return true;
    }

    public void Clear() => _pairs.Clear();

    public bool ContainsKey(TKey key) => !Unsafe.IsNullRef(in FindRef(key));

    public bool Remove(TKey key)
    {
        ref readonly var item = ref FindRef(key);
        if (Unsafe.IsNullRef(in item))
            return false;

        RemoveRef(in item);
        return true;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        ref readonly var item = ref FindRef(key);
        if (Unsafe.IsNullRef(in item)) {
            value = default;
            return false;
        }
        value = item.Value;
        return true;
    }

    private ref (TKey Key, TValue Value) FindRef(TKey key)
    {
        if (typeof(TKey).IsValueType && _comparer is null) {
            foreach (ref var pair in CollectionsMarshal.AsSpan(_pairs)) {
                if (EqualityComparer<TKey>.Default.Equals(pair.Key, key))
                    return ref pair;
            }
        }
        else {
            _comparer ??= EqualityComparer<TKey>.Default;
            foreach (ref var pair in CollectionsMarshal.AsSpan(_pairs)) {
                if (_comparer.Equals(pair.Key, key))
                    return ref pair;
            }
        }
        return ref Unsafe.NullRef<(TKey, TValue)>();
    }

    private ref TValue FindRefOrAddDefault(TKey key, out bool exist)
    {
        if (typeof(TKey).IsValueType && _comparer is null) {
            foreach (ref var pair in CollectionsMarshal.AsSpan(_pairs)) {
                if (EqualityComparer<TKey>.Default.Equals(pair.Key, key)) {
                    exist = true;
                    return ref pair.Value;
                }
            }
        }
        else {
            _comparer ??= EqualityComparer<TKey>.Default;
            foreach (ref var pair in CollectionsMarshal.AsSpan(_pairs)) {
                if (_comparer.Equals(pair.Key, key)) {
                    exist = true;
                    return ref pair.Value;
                }
            }
        }
        _pairs.Add((key, default!));
        exist = false;
        return ref CollectionsMarshal.AsSpan(_pairs)[^1].Value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item">Must be item in <see cref="_pairs"/></param>
    private void RemoveRef(ref readonly (TKey, TValue) item)
    {
        var index = CollectionsMarshal.AsSpan(_pairs).OffsetOf(in item);
        _pairs.RemoveAt(index);
    }

    public Enumerator GetEnumerator() => new(this);

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        ref readonly var findItem = ref FindRef(item.Key);
        if (Unsafe.IsNullRef(in findItem))
            return false;

        return EqualityComparer<TValue>.Default.Equals(findItem.Value, item.Value);
    }
    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        Guard.HasSizeGreaterThanOrEqualTo(array, arrayIndex + Count);

        if (Count == 0)
            return;

        foreach (var (k, v) in _pairs) {
            array[arrayIndex++] = KeyValuePair.Create(k, v);
        }
    }
    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        ref readonly var findItem = ref FindRef(item.Key);
        if (Unsafe.IsNullRef(in findItem))
            return false;
        if (!EqualityComparer<TValue>.Default.Equals(findItem.Value, item.Value))
            return false;

        RemoveRef(in findItem);
        return true;
    }
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private List<(TKey, TValue)>.Enumerator _enumerator;

        internal Enumerator(ListDictionary<TKey, TValue> dictionary)
            => _enumerator = dictionary._pairs.GetEnumerator();

        public KeyValuePair<TKey, TValue> Current
        {
            get {
                var current = _enumerator.Current;
                return KeyValuePair.Create(current.Item1, current.Item2);
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose() => _enumerator.Dispose();
        public bool MoveNext() => _enumerator.MoveNext();
        void IEnumerator.Reset() => ((IEnumerator)_enumerator).Reset();
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
            ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
            Guard.HasSizeGreaterThanOrEqualTo(array, arrayIndex + Count);

            if (Count == 0)
                return;

            foreach (var (k, _) in _dict._pairs) {
                array[arrayIndex++] = k;
            }
        }

        public Enumerator GetEnumerator() => new(this);

        void ICollection<TKey>.Add(TKey item) => ThrowHelper.ThrowNotSupportedException();
        void ICollection<TKey>.Clear() => ThrowHelper.ThrowNotSupportedException();
        IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => GetEnumerator();
        bool ICollection<TKey>.Remove(TKey item) => false;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<TKey>
        {
            private List<(TKey, TValue)>.Enumerator _enumerator;

            internal Enumerator(KeyCollection keys)
                => _enumerator = keys._dict._pairs.GetEnumerator();

            public TKey Current => _enumerator.Current.Item1;

            object IEnumerator.Current => Current;

            public void Dispose() => _enumerator.Dispose();
            public bool MoveNext() => _enumerator.MoveNext();
            void IEnumerator.Reset() => ((IEnumerator)_enumerator).Reset();
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
            ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
            Guard.HasSizeGreaterThanOrEqualTo(array, arrayIndex + Count);

            if (Count == 0)
                return;

            foreach (var (_, v) in _dict._pairs) {
                array[arrayIndex++] = v;
            }
        }

        public Enumerator GetEnumerator() => new(this);

        void ICollection<TValue>.Add(TValue item) => ThrowHelper.ThrowNotSupportedException();
        void ICollection<TValue>.Clear() => ThrowHelper.ThrowNotSupportedException();
        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => GetEnumerator();
        bool ICollection<TValue>.Remove(TValue item) => false;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<TValue>
        {
            private List<(TKey, TValue)>.Enumerator _enumerator;

            internal Enumerator(ValueCollection values)
                => _enumerator = values._dict._pairs.GetEnumerator();

            public TValue Current => _enumerator.Current.Item2;

            object? IEnumerator.Current => Current;

            public void Dispose() => _enumerator.Dispose();
            public bool MoveNext() => _enumerator.MoveNext();
            void IEnumerator.Reset() => ((IEnumerator)_enumerator).Reset();
        }
    }
}
