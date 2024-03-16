using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.AllocOpt.Providers;

namespace Trarizon.Library.Collections.AllocOpt;
public struct AllocOptDictionary<TKey, TValue, TComparer> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
    where TComparer : IEqualityComparer<TKey>
{
    private AllocOptHashSetProvider<(TKey Key, TValue Value), TKey, ByKeyComparer> _provider;

    public AllocOptDictionary(in TComparer comparer)
        => _provider = new(new ByKeyComparer(comparer));

    public AllocOptDictionary(int capacity, in TComparer comparer)
        => _provider = new(capacity, new ByKeyComparer(comparer));

    #region Accessors

    public readonly int Count => _provider.Count;

    public readonly int Capacity => _provider.Capacity;

    public TValue this[TKey key]
    {
        readonly get {
            ref readonly var val = ref GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(in val))
                ThrowHelper.ThrowKeyNotFound(key?.ToString() ?? string.Empty);

            return val!;
        }
        set {
            GetValueRefOrAddDefault(key) = value;
        }
    }

    public readonly ref readonly KeyCollection Keys
        => ref Unsafe.As<AllocOptDictionary<TKey, TValue, TComparer>, KeyCollection>(ref Unsafe.AsRef(in this));

    public readonly ref readonly ValueCollection Values
        => ref Unsafe.As<AllocOptDictionary<TKey, TValue, TComparer>, ValueCollection>(ref Unsafe.AsRef(in this));

    public readonly bool ContainsKey(TKey key)
        => !Unsafe.IsNullRef(in _provider.GetItemRefOrNullRef(key));

    public readonly bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        ref readonly var val = ref GetValueRefOrNullRef(key);
        if (Unsafe.IsNullRef(in val)) {
            value = default;
            return false;
        }

        value = val!;
        return true;
    }

    public readonly ref TValue? GetValueRefOrNullRef(TKey key)
        => ref _provider.GetItemRefOrNullRef(key).Value!;

    public readonly Enumerator GetEnumerator() => new(this);

    #endregion

    #region Builders

    public void Add(TKey key, TValue value)
    {
        if (!TryAdd(key, value))
            ThrowHelper.ThrowArgument("Key duplicated", nameof(key));
        return;
    }

    public bool TryAdd(TKey key, TValue value)
    {
        ref var item = ref _provider.GetItemRefOrAddEntry(key, returnNullIfExisting: true);
        if (Unsafe.IsNullRef(in item))
            return false;

        item = (key, value);
        return true;
    }

    public bool Remove(TKey key)
        => !Unsafe.IsNullRef(in _provider.GetItemRefAndRemove(key));

    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        ref readonly var entry = ref _provider.GetItemRefAndRemove(key);
        if (Unsafe.IsNullRef(in entry)) {
            value = default;
            return false;
        }

        value = entry.Value;
        return true;
    }

    public ref TValue? GetValueRefOrAddDefault(TKey key)
        => ref _provider.GetItemRefOrAddEntry(key, returnNullIfExisting: false).Value!;

    /// <remarks>
    /// This method won't clear elements in underlying array.
    /// Use <see cref="ClearUnreferenced"/> if you need it.
    /// </remarks>
    public void Clear() => _provider.Clear();

    public void ClearUnreferenced() => _provider.ClearUnreferenced();

    public void EnsureCapacity(int capacity) => _provider.EnsureCapacity(capacity);

    #endregion

    #region Explicit interface methods

    public readonly bool IsReadOnly => false;

    readonly ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
    readonly ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
    readonly IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
    readonly IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    readonly bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        ref readonly var value = ref GetValueRefOrNullRef(item.Key);
        if (Unsafe.IsNullRef(in value))
            return false;

        return EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }
    readonly void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(arrayIndex, array.Length - Count);

        if (Count == 0)
            return;

        foreach (ref readonly var item in _provider) {
            array[arrayIndex++] = KeyValuePair.Create(item.Key, item.Value);
        }
    }
    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        ref readonly var val = ref GetValueRefOrNullRef(item.Key);
        if (Unsafe.IsNullRef(in val) || !EqualityComparer<TValue>.Default.Equals(val, item.Value))
            return false;

        Remove(item.Key);
        return true;
    }
    readonly IEnumerator IEnumerable.GetEnumerator() => new Enumerator.Wrapper(this);
    readonly IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => new Enumerator.Wrapper(this);

    #endregion

    public struct Enumerator
    {
        private AllocOptHashSetProvider<(TKey, TValue), TKey, ByKeyComparer>.Enumerator _enumerator;

        internal Enumerator(in AllocOptDictionary<TKey, TValue, TComparer> dict)
            => _enumerator = dict._provider.GetEnumerator();

        public readonly KeyValuePair<TKey, TValue> Current
        {
            get {
                ref readonly var val = ref CurrentEntryValue;
                return KeyValuePair.Create(val.Key, val.Value);
            }
        }

        internal readonly ref readonly (TKey Key, TValue Value) CurrentEntryValue => ref _enumerator.Current;

        public bool MoveNext() => _enumerator.MoveNext();

        public void Reset() => _enumerator.Reset();

        internal sealed class Wrapper(in AllocOptDictionary<TKey, TValue, TComparer> dict) : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private Enumerator _enumerator = dict.GetEnumerator();

            public KeyValuePair<TKey, TValue> Current => _enumerator.Current;

            object? IEnumerator.Current => Current;

            public void Dispose() { }
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator.Reset();
        }
    }

    // Layout same as AllocOptDictionary`3
    public readonly struct KeyCollection : ICollection<TKey>, IReadOnlyCollection<TKey>
    {
        private readonly AllocOptDictionary<TKey, TValue, TComparer> _dict;

        public int Count => _dict.Count;

        public Enumerator GetEnumerator() => new(this);

        public bool Contains(TKey item) => _dict.ContainsKey(item);

        public void CopyTo(TKey[] array, int arrayIndex)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(arrayIndex, array.Length - Count);

            if (Count == 0)
                return;

            foreach (ref readonly var item in _dict._provider) {
                array[arrayIndex++] = item.Key;
            }
        }

        #region Explicit interface methods

        bool ICollection<TKey>.IsReadOnly => true;

        void ICollection<TKey>.Add(TKey item) => ThrowHelper.ThrowNotSupport(ThrowConstants.CollectionIsReadOnly);
        void ICollection<TKey>.Clear() => ThrowHelper.ThrowNotSupport(ThrowConstants.CollectionIsReadOnly);
        bool ICollection<TKey>.Remove(TKey item)
        {
            ThrowHelper.ThrowNotSupport(ThrowConstants.CollectionIsReadOnly);
            return default;
        }

        IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => new Enumerator.Wrapper(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator.Wrapper(this);

        #endregion

        public struct Enumerator
        {
            private AllocOptDictionary<TKey, TValue, TComparer>.Enumerator _enumerator;

            internal Enumerator(in KeyCollection keys)
               => _enumerator = keys._dict.GetEnumerator();

            public readonly TKey Current => _enumerator.CurrentEntryValue.Key;
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator.Reset();

            // Keep sync with Enumerator
            internal sealed class Wrapper(in KeyCollection keys) : IEnumerator<TKey>
            {
                private Enumerator _enumerator = keys.GetEnumerator();

                public TKey Current => _enumerator.Current;

                object? IEnumerator.Current => Current;

                public void Dispose() { }
                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset() => _enumerator.Reset();
            }
        }
    }

    // Layout same as AllocOptDictionary`3
    public readonly struct ValueCollection : ICollection<TValue>, IReadOnlyCollection<TValue>
    {
        private readonly AllocOptDictionary<TKey, TValue, TComparer> _dict;

        public int Count => _dict.Count;

        public Enumerator GetEnumerator() => new(this);

        public bool Contains(TValue item)
        {
            if (typeof(TValue).IsValueType) {
                foreach (var val in this) {
                    if (EqualityComparer<TValue>.Default.Equals(val, item))
                        return true;
                }
            }
            else {
                var comparer = EqualityComparer<TValue>.Default;
                foreach (var val in this) {
                    if (comparer.Equals(val, item))
                        return true;
                }
            }
            return false;
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(arrayIndex, array.Length - Count);

            if (Count == 0)
                return;

            foreach (ref readonly var item in _dict._provider) {
                array[arrayIndex++] = item.Value;
            }
        }

        #region Explicit interface methods

        bool ICollection<TValue>.IsReadOnly => true;

        void ICollection<TValue>.Add(TValue item) => ThrowHelper.ThrowNotSupport(ThrowConstants.CollectionIsReadOnly);
        void ICollection<TValue>.Clear() => ThrowHelper.ThrowNotSupport(ThrowConstants.CollectionIsReadOnly);
        bool ICollection<TValue>.Remove(TValue item)
        {
            ThrowHelper.ThrowNotSupport(ThrowConstants.CollectionIsReadOnly);
            return default;
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => new Enumerator.Wrapper(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator.Wrapper(this);

        #endregion

        public struct Enumerator
        {
            private AllocOptDictionary<TKey, TValue, TComparer>.Enumerator _enumerator;

            internal Enumerator(in ValueCollection values)
               => _enumerator = values._dict.GetEnumerator();

            public readonly TValue Current => _enumerator.Current.Value;
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator.Reset();

            // Keep sync with Enumerator
            internal sealed class Wrapper(in ValueCollection values) : IEnumerator<TValue>
            {
                private Enumerator _enumerator = values.GetEnumerator();

                public TValue Current => _enumerator.Current;

                object? IEnumerator.Current => Current;

                public void Dispose() { }
                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset() => _enumerator.Reset();
            }
        }
    }

    // Layout same as TComparer
    private struct ByKeyComparer(TComparer comparer) : IByKeyEqualityComparer<(TKey, TValue), TKey>
    {
        public bool Equals((TKey, TValue) val, TKey key) => comparer.Equals(val.Item1, key);

        public int GetHashCode([DisallowNull] TKey key) => comparer.GetHashCode(key);
    }
}

// This is a wrapper of AODictionary<TK, TV, IEqualityComparer<TK>>
[CollectionBuilder(typeof(AllocOptCollectionBuilder), nameof(AllocOptCollectionBuilder.CreateDictionary))]
public struct AllocOptDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    internal AllocOptDictionary<TKey, TValue, IEqualityComparer<TKey>> _dict;

    public AllocOptDictionary() : this(null) { }

    public AllocOptDictionary(IEqualityComparer<TKey>? comparer)
        => _dict = new(comparer ?? EqualityComparer<TKey>.Default);

    public AllocOptDictionary(int capacity, IEqualityComparer<TKey>? comparer = null)
        => _dict = new(capacity, comparer ?? EqualityComparer<TKey>.Default);

    #region Accessors

    public readonly int Count => _dict.Count;

    public readonly int Capacity => _dict.Capacity;

    public TValue this[TKey key]
    {
        readonly get => _dict[key];
        set => _dict[key] = value;
    }

    public readonly ref readonly AllocOptDictionary<TKey, TValue, IEqualityComparer<TKey>>.KeyCollection Keys => ref _dict.Keys;

    public readonly ref readonly AllocOptDictionary<TKey, TValue, IEqualityComparer<TKey>>.ValueCollection Values => ref _dict.Values;

    public readonly bool ContainsKey(TKey key) => _dict.ContainsKey(key);

    public readonly bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _dict.TryGetValue(key, out value);

    public readonly ref TValue? GetValueRefOrNullRef(TKey key) => ref _dict.GetValueRefOrNullRef(key);

    public readonly AllocOptDictionary<TKey, TValue, IEqualityComparer<TKey>>.Enumerator GetEnumerator() => _dict.GetEnumerator();

    #endregion

    #region Builders

    public void Add(TKey key, TValue value) => _dict.Add(key, value);

    public bool TryAdd(TKey key, TValue value) => _dict.TryAdd(key, value);

    public bool Remove(TKey key) => _dict.Remove(key);

    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value) => _dict.Remove(key, out value);

    public ref TValue? GetValueRefOrAddDefault(TKey key) => ref _dict.GetValueRefOrAddDefault(key);

    /// <remarks>
    /// This method won't clear elements in underlying array.
    /// Use <see cref="ClearUnreferenced"/> if you need it.
    /// </remarks>
    public void Clear() => _dict.Clear();

    public void ClearUnreferenced() => _dict.ClearUnreferenced();

    public void EnsureCapacity(int capacity) => _dict.EnsureCapacity(capacity);

    #endregion

    #region Explicit interface methods

    public readonly bool IsReadOnly => false;

    readonly ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
    readonly ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
    readonly IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
    readonly IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    readonly bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_dict).Contains(item);
    readonly void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_dict).CopyTo(array, arrayIndex);
    readonly bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_dict).Remove(item);
    readonly IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();
    readonly IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)_dict).GetEnumerator();

    #endregion
}