using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.AllocOpt.Providers;

namespace Trarizon.Library.Collections.AllocOpt;
public struct AllocOptDictionary<TKey, TValue, TComparer> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    where TComparer : IEqualityComparer<TKey>
{
    internal AllocOptHashMapProvider<TKey, TValue, TComparer> _provider;

    public AllocOptDictionary(TComparer comparer)
        => _provider = new(comparer);

    public AllocOptDictionary(int capacity, TComparer comparer)
        => _provider = new(capacity, comparer);

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
        => !Unsafe.IsNullRef(in _provider.GetEntryRefOrNullRef(key));

    public readonly bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        ref readonly var entry = ref _provider.GetEntryRefOrNullRef(key);
        if (Unsafe.IsNullRef(in entry)) {
            value = default;
            return false;
        }

        value = entry.Value;
        return true;
    }

    public readonly ref TValue? GetValueRefOrNullRef(TKey key)
        => ref _provider.GetEntryRefOrNullRef(key).Value!;

    public readonly Enumerator GetEnumerator() => new(this);

    #endregion

    #region Builders

    public void Add(TKey key, TValue value)
    {
        ref var entry = ref _provider.GetOrAddEntryRef(key, returnNullIfExisting: true);
        if (Unsafe.IsNullRef(in entry))
            ThrowHelper.ThrowArgument("Key duplicated", nameof(key));

        entry.Value = value;
    }

    public bool TryAdd(TKey key, TValue value)
    {
        ref var entry = ref _provider.GetOrAddEntryRef(key, returnNullIfExisting: true);
        if (Unsafe.IsNullRef(in entry))
            return false;

        entry.Value = value;
        return true;
    }

    public bool Remove(TKey key)
        => !Unsafe.IsNullRef(in _provider.GetAndRemoveEntryRef(key));

    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        ref readonly var entry = ref _provider.GetAndRemoveEntryRef(key);
        if (Unsafe.IsNullRef(in entry)) {
            value = default;
            return false;
        }

        value = entry.Value;
        return true;
    }

    public ref TValue? GetValueRefOrAddDefault(TKey key)
        => ref _provider.GetOrAddEntryRef(key, returnNullIfExisting: false).Value!;

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
    readonly void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _provider.CopyTo(array, arrayIndex);
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
        private AllocOptHashMapProvider<TKey, TValue, TComparer>.Enumerator _enumerator;

        internal Enumerator(in AllocOptDictionary<TKey, TValue, TComparer> dict)
            => _enumerator = new(dict._provider);

        public readonly KeyValuePair<TKey, TValue> Current
        {
            get {
                ref readonly var entry = ref _enumerator.Current;
                return KeyValuePair.Create(entry.Key, entry.Value);
            }
        }

        public bool MoveNext() => _enumerator.MoveNext();

        internal sealed class Wrapper(in AllocOptDictionary<TKey, TValue, TComparer> dict) : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private AllocOptHashMapProvider<TKey, TValue, TComparer>.Enumerator _enumerator = new(dict._provider);

            public KeyValuePair<TKey, TValue> Current
            {
                get {
                    ref readonly var entry = ref _enumerator.Current;
                    return KeyValuePair.Create(entry.Key, entry.Value);
                }
            }

            object? IEnumerator.Current => Current;

            public void Dispose() { }
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator.Reset();
        }
    }

    // Layout same as AllocOptDictionary<,,>
    public readonly struct KeyCollection : ICollection<TKey>, IReadOnlyCollection<TKey>
    {
        private readonly AllocOptDictionary<TKey, TValue, TComparer> _dict;

        public int Count => _dict.Count;

        public Enumerator GetEnumerator() => new(this);

        public bool Contains(TKey item) => _dict.ContainsKey(item);

        public void CopyTo(TKey[] array, int arrayIndex) => _dict._provider.CopyToKeyArray(array, arrayIndex);

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
            private AllocOptHashMapProvider<TKey, TValue, TComparer>.Enumerator _enumerator;

            internal Enumerator(in KeyCollection keys)
               => _enumerator = new(keys._dict._provider);

            public readonly TKey Current => _enumerator.Current.Key;

            public bool MoveNext() => _enumerator.MoveNext();

            // Keep sync with Enumerator
            internal sealed class Wrapper(in KeyCollection keys) : IEnumerator<TKey>
            {
                private AllocOptHashMapProvider<TKey, TValue, TComparer>.Enumerator _enumerator = new(keys._dict._provider);

                public TKey Current => _enumerator.Current.Key;

                object? IEnumerator.Current => Current;

                public void Dispose() { }
                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset() => _enumerator.Reset();
            }
        }
    }

    // Layout same as AllocOptDictionary<,,>
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

        public void CopyTo(TValue[] array, int arrayIndex) => _dict._provider.CopyToValueArray(array, arrayIndex);

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
            private AllocOptHashMapProvider<TKey, TValue, TComparer>.Enumerator _enumerator;

            internal Enumerator(in ValueCollection values)
               => _enumerator = new(values._dict._provider);

            public readonly TValue Current => _enumerator.Current.Value;

            public bool MoveNext() => _enumerator.MoveNext();

            // Keep sync with Enumerator
            internal sealed class Wrapper(in ValueCollection values) : IEnumerator<TValue>
            {
                private AllocOptHashMapProvider<TKey, TValue, TComparer>.Enumerator _enumerator = new(values._dict._provider);

                public TValue Current => _enumerator.Current.Value;

                object? IEnumerator.Current => Current;

                public void Dispose() { }
                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset() => _enumerator.Reset();
            }
        }
    }
}

// This is a wrapper of AODictionary<TK, TV, WrappedEqualityComparer<TK>>
[CollectionBuilder(typeof(AllocOptCollectionBuilder), nameof(AllocOptCollectionBuilder.CreateDictionary))]
public struct AllocOptDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
{
    internal AllocOptDictionary<TKey, TValue, WrappedEqualityComparer<TKey>> _dict;

    public AllocOptDictionary()
        => _dict = new(default);

    public AllocOptDictionary(int capacity)
        => _dict = new(capacity, default);

    public AllocOptDictionary(IEqualityComparer<TKey> comparer)
        => _dict = new(Unsafe.As<IEqualityComparer<TKey>, WrappedEqualityComparer<TKey>>(ref comparer));

    public AllocOptDictionary(int capacity, IEqualityComparer<TKey> comparer)
        => _dict = new(capacity, Unsafe.As<IEqualityComparer<TKey>, WrappedEqualityComparer<TKey>>(ref comparer));

    #region Accessors

    public readonly int Count => _dict.Count;

    public readonly int Capacity => _dict.Capacity;

    public TValue this[TKey key]
    {
        readonly get => _dict[key];
        set => _dict[key] = value;
    }

    public readonly ref readonly AllocOptDictionary<TKey, TValue, WrappedEqualityComparer<TKey>>.KeyCollection Keys => ref _dict.Keys;

    public readonly ref readonly AllocOptDictionary<TKey, TValue, WrappedEqualityComparer<TKey>>.ValueCollection Values => ref _dict.Values;

    public readonly bool ContainsKey(TKey key) => _dict.ContainsKey(key);

    public readonly bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _dict.TryGetValue(key, out value);

    public readonly ref TValue? GetValueRefOrNullRef(TKey key) => ref _dict.GetValueRefOrNullRef(key);

    public readonly AllocOptDictionary<TKey, TValue, WrappedEqualityComparer<TKey>>.Enumerator GetEnumerator() => _dict.GetEnumerator();

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
    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_dict).Remove(item);
    readonly IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();
    readonly IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)_dict).GetEnumerator();

    #endregion
}