using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.AllocOpt;

/// <summary>
/// Minimal dictionary use pooled array as underlying array
/// DO NOT reassign this struct
/// </summary>
public struct RentedDictionary<TKey, TValue> : IDisposable where TKey : notnull
{
    private Entry[]? _entries;
    private int[]? _buckets;
    private int _bucketLength; // actual length of _buckets ans _entries, as length of rented array may not be prime
    private int _count;
    private int _freeHead;
    private int _freeCount;
    private readonly IEqualityComparer<TKey>? _comparer;

    private const int StartOfFreeList = -3;

    public RentedDictionary() : this(0, null) { }
    public RentedDictionary(int minInitialCapacity) : this(minInitialCapacity, null) { }
    public RentedDictionary(IEqualityComparer<TKey>? comparer) : this(0, comparer) { }
    public RentedDictionary(int minInitialCapacity, IEqualityComparer<TKey>? comparer)
    {
        if (minInitialCapacity > 0) {
            Initialize(minInitialCapacity);
        }

        if (!typeof(TKey).IsValueType) {
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
        }
        else if (_comparer is not null && _comparer != EqualityComparer<TKey>.Default) {
            _comparer = comparer;
        }
    }

    public readonly IEqualityComparer<TKey> Comparer => _comparer ?? EqualityComparer<TKey>.Default;
    public readonly int Count => _count - _freeCount;
    public readonly int Capacity => _bucketLength;
    public TValue this[TKey key]
    {
        readonly get {
            ref var entry = ref FindEntryRef(key);
            if (Unsafe.IsNullRef(ref entry)) {
                Throws.KeyNotFound(key, nameof(RentedDictionary<,>));
            }
            return entry.value;
        }
        set {
            TryAddInternal(key, value, DictionaryKeyExistingBehaviour.Overwrite);
        }
    }

    public void Set(TKey key, TValue value)
        => this[key] = value;

    public void Add(TKey key, TValue value)
    {
        TryAddInternal(key, value, DictionaryKeyExistingBehaviour.Throw);
    }

    public bool TryAdd(TKey key, TValue value)
        => TryAddInternal(key, value, DictionaryKeyExistingBehaviour.None);

    public void Clear()
    {
        if (_count > 0) {
            Debug.Assert(_buckets is not null);
            Debug.Assert(_entries is not null);
            Array.Clear(_buckets, 0, _bucketLength);
            Array.Clear(_entries, 0, _bucketLength);
            _count = 0;
            _freeHead = -1;
            _freeCount = 0;
        }
    }

    public readonly bool ContainsKey(TKey key)
        => Unsafe.IsNullRef(ref FindEntryRef(key)) is false;

    public readonly bool ContainsValue(TValue value)
    {
        var entries = _entries.AsSpan(0, _count);
        if (value == null) {
            foreach (ref readonly var entry in entries) {
                if (entry.next >= -1 && entry.value == null) {
                    return true;
                }
            }
        }
        else if (typeof(TValue).IsValueType) {
            foreach (ref readonly var entry in entries) {
                if (entry.next >= -1 && EqualityComparer<TValue>.Default.Equals(entry.value, value)) {
                    return true;
                }
            }
        }
        else {
            var comparer = EqualityComparer<TValue>.Default;
            foreach (ref readonly var entry in entries) {
                if (entry.next >= -1 && comparer.Equals(entry.value, value)) {
                    return true;
                }
            }
        }
        return false;
    }

    public bool Remove(TKey key) => Remove(key, out _);

    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (_buckets is null) {
            value = default;
            return false;
        }

        var hashCode = (uint)key.GetHashCode();
        ref var bucket = ref GetBucket(hashCode);
        var i = bucket - 1;
        var entries = _entries.AsSpan(0, _bucketLength);
        var last = -1;

        while ((uint)i < (uint)_bucketLength) {
            ref var entry = ref entries[i];
            if (entry.hashCode == hashCode &&
                (typeof(TKey).IsValueType && _comparer is null ? EqualityComparer<TKey>.Default.Equals(entry.key, key) : _comparer!.Equals(entry.key, key))) {
                if (last < 0) {
                    bucket = entry.next + 1;
                }
                else {
                    entries[last].next = entry.next;
                }

                value = entry.value;

                Debug.Assert((StartOfFreeList - _freeHead) < 0);
                entry.next = StartOfFreeList - _freeHead;

                if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>()) {
                    entry.key = default!;
                }
                if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>()) {
                    entry.value = default!;
                }

                _freeHead = i;
                _freeCount++;
                return true;
            }

            last = i;
            i = entry.next;
        }

        value = default;
        return false;
    }

    public readonly bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        ref var entry = ref FindEntryRef(key);
        if (Unsafe.IsNullRef(ref entry)) {
            value = default!;
            return false;
        }
        value = entry.value;
        return true;
    }

    public int EnsureCapacity(int capacity)
    {
        int currentCapacity = _bucketLength;
        if (currentCapacity >= capacity) {
            return currentCapacity;
        }

        if (_buckets is null) {
            return Initialize(capacity);
        }

        int newSize = HashHelpers.GetPrime(capacity);
        Resize(newSize);
        return newSize;
    }

    private readonly ref Entry FindEntryRef(TKey key)
    {
        ref Entry entry = ref Unsafe.NullRef<Entry>();
        if (_buckets is null)
            goto NotFound;

        if (typeof(TKey).IsValueType && _comparer is null) {
            var hashCode = (uint)key.GetHashCode();
            var i = GetBucket(hashCode) - 1;
            var entries = _entries.AsSpan(0, _bucketLength);

            while ((uint)i < (uint)_bucketLength) {
                entry = ref entries[i];
                if (entry.hashCode == hashCode && EqualityComparer<TKey>.Default.Equals(entry.key, key)) {
                    return ref entry;
                }
                i = entries[i].next;
            }
        }
        else {
            Debug.Assert(_comparer is not null);
            var hashCode = (uint)_comparer!.GetHashCode(key);
            var i = GetBucket(hashCode) - 1;
            var entries = _entries.AsSpan(0, _bucketLength);

            while ((uint)i < (uint)_bucketLength) {
                entry = ref entries[i];
                if (entry.hashCode == hashCode && _comparer.Equals(entry.key, key)) {
                    return ref entry;
                }
                i = entries[i].next;
            }
        }

    NotFound:
        return ref Unsafe.NullRef<Entry>();
    }

    private bool TryAddInternal(TKey key, TValue value, DictionaryKeyExistingBehaviour behaviour)
    {
        if (_buckets is null) {
            Initialize(0);
        }
        Debug.Assert(_entries is not null);
        var entries = _entries.AsSpan(0, _bucketLength);
        uint hashCode;
        ref int bucket = ref Unsafe.NullRef<int>();

        if (typeof(TKey).IsValueType && _comparer is null) {
            hashCode = (uint)key.GetHashCode();
            bucket = ref GetBucket(hashCode);
            int i = bucket - 1;

            while ((uint)i < (uint)_bucketLength) {
                ref var curEntry = ref entries[i];
                if (curEntry.hashCode == hashCode && EqualityComparer<TKey>.Default.Equals(curEntry.key, key)) {
                    switch (behaviour) {
                        case DictionaryKeyExistingBehaviour.Overwrite:
                            curEntry.value = value;
                            return true;
                        case DictionaryKeyExistingBehaviour.Throw:
                            Throws.KeyAlreadyExists(key, nameof(RentedDictionary<,>));
                            break;
                        case DictionaryKeyExistingBehaviour.None:
                        default:
                            return false;
                    }
                }

                i = curEntry.next;
            }
        }
        else {
            Debug.Assert(_comparer is not null);
            hashCode = (uint)_comparer!.GetHashCode(key);
            bucket = ref GetBucket(hashCode);
            int i = bucket - 1;

            while ((uint)i < (uint)_bucketLength) {
                ref var curEntry = ref entries[i];
                if (curEntry.hashCode == hashCode && _comparer.Equals(curEntry.key, key)) {
                    switch (behaviour) {
                        case DictionaryKeyExistingBehaviour.Overwrite:
                            curEntry.value = value;
                            return true;
                        case DictionaryKeyExistingBehaviour.Throw:
                            Throws.KeyAlreadyExists(key, nameof(RentedDictionary<,>));
                            break;
                        case DictionaryKeyExistingBehaviour.None:
                        default:
                            return false;
                    }
                }

                i = curEntry.next;
            }
        }

        // Not found
        int index;
        if (_freeCount > 0) {
            index = _freeHead;
            _freeHead = StartOfFreeList - entries[_freeHead].next;
            _freeCount--;
        }
        else {
            if (_count == _bucketLength) {
                Resize();
                bucket = ref GetBucket(hashCode);
            }
            index = _count;
            _count++;
            entries = _entries.AsSpan(0, _bucketLength);
        }

        ref Entry entry = ref entries[index];
        entry.hashCode = hashCode;
        entry.key = key;
        entry.value = value;
        entry.next = bucket - 1;
        bucket = index + 1;

        return true;
    }

    private void Resize() => Resize(HashHelpers.ExpandPrime(_count));

    private void Resize(int newSize)
    {
        Debug.Assert(_entries is not null);
        Debug.Assert(_buckets is not null);
        Debug.Assert(newSize >= _bucketLength);

        var entries = ArrayPool<Entry>.Shared.Rent(newSize);
        var buckets = ArrayPool<int>.Shared.Rent(newSize);
        Array.Clear(buckets, 0, newSize);
        Array.Copy(_entries, entries, _count);
        ArrayPool<Entry>.Shared.Return(_entries, RuntimeHelpers.IsReferenceOrContainsReferences<TKey>() || RuntimeHelpers.IsReferenceOrContainsReferences<TValue>());
        ArrayPool<int>.Shared.Return(_buckets);

        _entries = entries;
        _buckets = buckets;
        _bucketLength = newSize;

        for (int i = 0; i < _count; i++) {
            ref Entry entry = ref _entries[i];
            if (entry.next >= -1) {
                ref int bucket = ref GetBucket(entry.hashCode);
                entry.next = bucket - 1;
                bucket = i + 1;
            }
        }
    }

    [MemberNotNull(nameof(_buckets), nameof(_entries))]
    private int Initialize(int capacity)
    {
        Debug.Assert(_buckets is null);
        Debug.Assert(_entries is null);
        int size = HashHelpers.GetPrime(capacity);
        _buckets = ArrayPool<int>.Shared.Rent(size);
        Array.Clear(_buckets, 0, size);
        _bucketLength = size;
        _freeHead = -1;
        _entries = ArrayPool<Entry>.Shared.Rent(size);
        Array.Clear(_entries, 0, size);
        return size;
    }

    private readonly ref int GetBucket(uint hashCode)
    {
        return ref _buckets![hashCode % _bucketLength];
    }

    public void Dispose()
    {
        if (_buckets is not null) {
            ArrayPool<int>.Shared.Return(_buckets);
        }
        if (_entries is not null) {
            ArrayPool<Entry>.Shared.Return(_entries, RuntimeHelpers.IsReferenceOrContainsReferences<TKey>() || RuntimeHelpers.IsReferenceOrContainsReferences<TValue>());
        }
        _entries = null!;
        _buckets = null!;
        _bucketLength = 0;
        _count = 0;
    }

    private struct Entry
    {
        public uint hashCode;
        /// <summary>
        /// 0-based index of next entry in chain: -1 means end of chain
        /// also encodes whether this entry _itself_ is part of the free list by changing sign and subtracting 3,
        /// so -2 means end of free list, -3 means index 0 but on free list, -4 means index 1 but on free list, etc.
        /// </summary>
        public int next;
        public TKey key;     // Key of entry
        public TValue value; // Value of entry
    }
}
