using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.Extensions;

namespace Trarizon.Library.Collections.AllocOpt.Providers;
internal struct AllocOptHashMapProvider<TKey, TValue, TComparer> where TComparer : IEqualityComparer<TKey>
{
    private int[] _buckets;
    private Entry[] _entries;
    private int _size;
    private int _freeListHead;
    private int _freeListCount;

    private readonly TComparer _comparer;

    public AllocOptHashMapProvider(TComparer comparer)
    {
        _buckets = [];
        _entries = [];
        _freeListHead = -1;
        _comparer = comparer;
    }

    public AllocOptHashMapProvider(int capacity, TComparer comparer)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 0);
        if (capacity == 0) {
            _buckets = [];
            _entries = [];
        }
        else
            Initialize(capacity);
        _freeListHead = -1;
        _comparer = comparer;
    }

    #region Accessors

    public readonly int Count => _size - _freeListCount;

    public readonly int Size => _size;

    public readonly int Capacity => _entries.Length;

    public readonly ref Entry GetEntryRefOrNullRef(TKey key)
    {
        if (_entries.Length == 0)
            return ref Unsafe.NullRef<Entry>();

        uint hashCode = GetHashCode(key);
        int index = GetBucketRef(hashCode) - 1;

        while (index >= 0) {
            ref Entry entry = ref _entries[index];
            if (entry.HashCode == hashCode && _comparer.Equals(entry.Key, key)) {
                return ref entry;
            }
            index = entry.Next;
        }
        return ref Unsafe.NullRef<Entry>();
    }

    public readonly int GetInternalEntryIndex(ref readonly Entry entry)
    {
        return _entries.AsSpan().OffsetOf(in entry);
    }

    #endregion

    #region Builders

    /// <remarks>
    /// This method won't clear elements in underlying array.
    /// Use <see cref="ClearUnreferenced"/> if you need it.
    /// </remarks>
    public void Clear()
    {
        if (_size == 0)
            return;

        _size = 0;
        _freeListHead = -1;
        Array.Clear(_buckets);
    }

    public void ClearUnreferenced()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>() ||
            RuntimeHelpers.IsReferenceOrContainsReferences<TValue>()) {
            if (_size > 0) {
                Array.Clear(_entries, _size, _entries.Length - _size);
                _size = 0;

                if (_freeListCount > 0) {
                    Debug.Assert(_freeListHead >= 0);
                    int index = _freeListHead;
                    while (index >= 0) {
                        ref Entry entry = ref _entries[index];
                        entry.Key = default!;
                        entry.Value = default!;
                        index = entry.Next;
                    }
                }
            }
        }
    }

    public void EnsureCapacity(int capacity)
    {
        if (capacity <= _entries.Length)
            return;

        if (_entries.Length == 0) {
            Initialize(capacity);
            return;
        }

        Grow(HashHelper.GetPrime(capacity));
        return;
    }

    public ref readonly Entry GetAndRemoveEntryRef(TKey key)
    {
        if (_entries.Length == 0)
            return ref Unsafe.NullRef<Entry>();

        uint hashCode = GetHashCode(key);
        ref int bucket = ref GetBucketRef(hashCode);
        int index = bucket - 1; // bucket 1-based
        int prev = -1;

        while (index >= 0) {
            ref Entry entry = ref _entries[index];
            if (entry.HashCode == hashCode && _comparer.Equals(entry.Key, key)) {
                if (prev < 0) {
                    bucket = entry.Next + 1;
                }
                else {
                    _entries[prev].Next = entry.Next;
                }
                entry.NextAsInFreeList = _freeListHead;
                _freeListHead = index;
                _freeListCount++;
                return ref entry;
            }
            prev = index;
            index = entry.Next;
        }
        return ref Unsafe.NullRef<Entry>();
    }

    public ref Entry GetOrAddEntryRef(TKey key, bool returnNullIfExisting)
    {
        if (_entries.Length == 0)
            Initialize(1);

        uint hashCode = GetHashCode(key);
        ref int bucket = ref GetBucketRef(hashCode);
        int index = bucket - 1;

        while (index >= 0) {
            ref Entry entry = ref _entries[index];
            if (entry.HashCode == hashCode && _comparer.Equals(entry.Key, key)) {
                return ref returnNullIfExisting
                    ? ref Unsafe.NullRef<Entry>()
                    : ref entry;
            }
            index = entry.Next;
        }

        if (_freeListCount > 0) {
            // Use free list
            index = _freeListHead;
            _freeListHead = _entries[_freeListHead].NextAsInFreeList;
            _freeListCount--;
        }
        else {
            if (_size == _entries.Length) {
                Grow(HashHelper.ExpandPrime(_size));
                bucket = ref GetBucketRef(hashCode);
            }
            index = _size;
            _size++;
        }

        ref Entry result = ref _entries[index];
        result.HashCode = hashCode;
        result.Next = bucket - 1;
        result.Key = key;
        bucket = index + 1;

        return ref result;
    }

    private void Grow(int newSize)
    {
        _buckets = new int[newSize];
        var oldEntries = _entries;
        _entries = new Entry[newSize];
        Array.Copy(oldEntries, _entries, _size);

        for (int i = 0; i < _size; i++) {
            ref Entry entry = ref _entries[i];
            if (entry.IsActive) {
                ref int bucket = ref GetBucketRef(entry.HashCode);
                entry.Next = bucket;
                bucket = i + 1;
            }
        }
    }

    #endregion

    [MemberNotNull(nameof(_buckets), nameof(_entries))]
    private void Initialize(int expectedCapacity)
    {
        int size = HashHelper.GetPrime(expectedCapacity);
        _buckets = new int[size];
        _entries = new Entry[size];
    }

    private readonly ref int GetBucketRef(uint hashCode)
    {
        Debug.Assert(_buckets.Length > 0);
        return ref _buckets[hashCode % (uint)_buckets.Length];
    }

    private readonly uint GetHashCode(TKey key)
        => key is null ? 0u : (uint)_comparer.GetHashCode(key);

    #region Interface method helpers

    public readonly void CopyToKeyArray(TKey[] array, int arrayIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(arrayIndex, array.Length - Count);

        if (_size == 0)
            return;

        for (int i = 0; i < _size; i++) {
            ref Entry entry = ref _entries[i];
            if (entry.IsActive) {
                array[arrayIndex++] = entry.Key;
            }
        }
    }

    public readonly void CopyToValueArray(TValue[] array, int arrayIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(arrayIndex, array.Length - Count);

        if (_size == 0)
            return;

        for (int i = 0; i < _size; i++) {
            ref Entry entry = ref _entries[i];
            if (entry.IsActive) {
                array[arrayIndex++] = entry.Value;
            }
        }
    }

    public readonly void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(arrayIndex, array.Length - Count);

        if (_size == 0)
            return;

        for (int i = 0; i < _size; i++) {
            ref Entry entry = ref _entries[i];
            if (entry.IsActive) {
                array[arrayIndex++] = entry.ToKeyValuePair();
            }
        }
    }

    #endregion

    internal struct Entry
    {
        public uint HashCode;
        public int Next;
        public TKey Key;
        public TValue Value;

        public readonly bool IsActive => Next >= -1;
        public int NextAsInFreeList
        {
            readonly get => -3 - Next;
            set => Next = -3 - value;
        }

        public readonly KeyValuePair<TKey, TValue> ToKeyValuePair()
            => KeyValuePair.Create(Key, Value);
    }

    public struct Enumerator(in AllocOptHashMapProvider<TKey, TValue, TComparer> provider)
    {
        private readonly Entry[] _entries = provider._entries;
        private readonly int _size = provider._size;
        private int _index = -1;

        public readonly ref readonly Entry Current => ref _entries[_index];

        public bool MoveNext()
        {
            var index = _index + 1;
            while (index < _size) {
                if (_entries[index].IsActive) {
                    _index = index;
                    return true;
                }
                index++;
            }
            _index = index;
            return false;
        }

        public void Reset() => _index = -1;
    }

    private static class HashHelper
    {
        private static ReadOnlySpan<int> Primes => [3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369];

        public static int GetPrime(int min)
        {
            foreach (var prime in Primes) {
                if (prime >= min)
                    return prime;
            }

            // Outside of predefined
            for (int i = (min | 1); i < int.MaxValue; i += 2) {
                //                Well I copied it from BCL, what does this mod do?
                if (IsPrime(i) && ((i - 1) % 101 != 0))
                    return i;
            }
            return min;

            static bool IsPrime(int number)
            {
                if (number % 2 == 0)
                    return number == 2;

                int max = (int)Math.Sqrt(number);
                for (int divisor = 3; divisor < max; divisor += 2) {
                    if (number % divisor == 0)
                        return false;
                }
                return true;
            }
        }

        public static int ExpandPrime(int oldSize)
        {
            // The max prime smaller than Array.MaxSize
            const int MaxPrimeArrayLength = 0x7FFFFFC3;

            int newSize = 2 * oldSize;
            if ((uint)newSize > MaxPrimeArrayLength && MaxPrimeArrayLength > oldSize) {
                return MaxPrimeArrayLength;
            }
            return GetPrime(newSize);
        }
    }
}
