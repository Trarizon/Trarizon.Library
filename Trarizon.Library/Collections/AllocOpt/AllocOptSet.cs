using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.AllocOpt.Providers;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections.AllocOpt;
// Unlike HashSet<>, this collection doesn't check collision count
public struct AllocOptSet<T, TComparer> : ISet<T>, IReadOnlySet<T>
    where TComparer : IEqualityComparer<T>
{
    private AllocOptHashSetProvider<T, T, Comparer> _provider;

    public AllocOptSet(in TComparer comparer)
        => _provider = new(Unsafe.As<TComparer, Comparer>(ref Unsafe.AsRef(in comparer)));

    public AllocOptSet(int capacity, in TComparer comparer)
        => _provider = new(capacity, Unsafe.As<TComparer, Comparer>(ref Unsafe.AsRef(in comparer)));

    #region Accessors

    public readonly int Count => _provider.Count;

    public readonly int Capacity => _provider.Capacity;

    public readonly bool Contains(T item)
        => !Unsafe.IsNullRef(in _provider.GetItemRefOrNullRef(item));

    public readonly bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T value)
    {
        ref readonly var val = ref _provider.GetItemRefOrNullRef(equalValue);
        if (Unsafe.IsNullRef(in val)) {
            value = default;
            return false;
        }

        value = val;
        return true;
    }

    public readonly Enumerator GetEnumerator() => new(_provider);

    #endregion

    #region Builders

    public bool Add(T item)
    {
        ref var val = ref _provider.GetItemRefOrAddEntry(item, true);
        if (Unsafe.IsNullRef(in val))
            return false;

        val = item;
        return true;
    }

    public bool Remove(T item)
        => !Unsafe.IsNullRef(in _provider.GetItemRefAndRemove(item));

    /// <remarks>
    /// This method won't clear elements in underlying array.
    /// Use <see cref="ClearUnreferenced"/> if you need it.
    /// </remarks>
    public void Clear() => _provider.Clear();

    public void ClearUnreferenced() => _provider.ClearUnreferenced();

    public void EnsureCapacity(int capacity) => _provider.EnsureCapacity(capacity);

    #endregion

    #region Interface methods

    readonly bool ICollection<T>.IsReadOnly => false;

    public readonly void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(arrayIndex, array.Length - Count);

        if (Count == 0)
            return;

        foreach (ref readonly var val in _provider) {
            array[arrayIndex++] = val;
        }
    }

    /// <summary>
    /// a -= b<br/>
    /// Unlike BCL, this method doesn't check condition of except self
    /// </summary>
    public void ExceptWith(IEnumerable<T> other)
    {
        if (Count == 0)
            return;

        foreach (var item in other)
            Remove(item);
    }

    /// <summary>
    /// a &= b<br/>
    /// Unlike BCL, this method doesn't check condition of except self
    /// </summary>
    public void IntersectWith(IEnumerable<T> other)
    {
        if (Count == 0)
            return;

        if (other.TryGetNonEnumeratedCount(out var otherCount) && otherCount == 0) {
            Clear();
            return;
        }

        var bitArray = new StackAllocBitArray(
            stackalloc byte[StackAllocBitArray.GetArrayLength(_provider.Size)]);

        foreach (var key in other) {
            ref readonly var val = ref _provider.GetItemRefOrNullRef(key);
            if (!Unsafe.IsNullRef(in val))
                bitArray[_provider.GetInternalEntryIndex(in val)] = true;
        }

        foreach (ref readonly var val in _provider) {
            if (!bitArray[_provider.GetInternalEntryIndex(in val)])
                Remove(val);
        }
    }

    /// <summary>
    /// a &lt; b<br/>
    /// Unlike BCL, this method doesn't check condition of except self
    /// </summary>
    public readonly bool IsProperSubsetOf(IEnumerable<T> other)
    {
        if (other.TryGetNonEnumeratedCount(out var otherCount)) {
            if (otherCount == 0)
                return false;
            if (Count == 0)
                return otherCount > 0;
        }

        var bitArray = new StackAllocBitArray(
            stackalloc byte[StackAllocBitArray.GetArrayLength(_provider.Size)]);

        int found = 0;
        bool isProper = false;
        foreach (var key in other) {
            ref readonly var entry = ref _provider.GetItemRefOrNullRef(key);
            // Found item not in other, result maybe proper
            if (Unsafe.IsNullRef(in entry))
                isProper = true;
            else {
                var index = _provider.GetInternalEntryIndex(in entry);
                if (!bitArray[index]) {
                    bitArray[index] = true;
                    found++;
                }
            }
        }

        return isProper && found == Count;
    }

    /// <summary>
    /// a &gt; b<br/>
    /// Unlike BCL, this method doesn't check condition of except self
    /// </summary>
    public readonly bool IsProperSupersetOf(IEnumerable<T> other)
    {
        if (Count == 0)
            return false;

        if (other.TryGetNonEnumeratedCount(out var otherCount)) {
            if (otherCount == 0)
                return true;
        }

        var bitArray = new StackAllocBitArray(
            stackalloc byte[StackAllocBitArray.GetArrayLength(_provider.Size)]);

        int found = 0;
        foreach (var key in other) {
            ref readonly var entry = ref _provider.GetItemRefOrNullRef(key);
            // found item not in set, other is not subset of set
            if (Unsafe.IsNullRef(in entry))
                return false;
            else {
                var index = _provider.GetInternalEntryIndex(in entry);
                if (!bitArray[index]) {
                    bitArray[index] = true;
                    found++;
                }
            }
        }

        return found < Count;
    }

    /// <summary>
    /// a &lt;= b<br/>
    /// Unlike BCL, this method doesn't check condition of except self
    /// </summary>
    public readonly bool IsSubsetOf(IEnumerable<T> other)
    {
        if (Count == 0)
            return true;

        var bitArray = new StackAllocBitArray(
            stackalloc byte[StackAllocBitArray.GetArrayLength(_provider.Size)]);

        int found = 0;
        foreach (var key in other) {
            ref readonly var entry = ref _provider.GetItemRefOrNullRef(key);
            if (!Unsafe.IsNullRef(in entry)) {
                var index = _provider.GetInternalEntryIndex(in entry);
                if (!bitArray[index]) {
                    bitArray[index] = true;
                    found++;
                }
            }
        }

        return found == Count;
    }

    /// <summary>
    /// a &gt;= b<br/>
    /// Unlike BCL, this method doesn't check condition of except self
    /// </summary>
    public readonly bool IsSupersetOf(IEnumerable<T> other)
    {
        if (other.TryGetNonEnumeratedCount(out var otherCount)) {
            if (otherCount == 0)
                return true;
        }

        foreach (var key in other) {
            if (!Contains(key))
                return false;
        }

        return true;
    }

    /// <summary>
    /// a & b &gt; 0<br/>
    /// Unlike BCL, this method doesn't check condition of except self
    /// </summary>
    public readonly bool Overlaps(IEnumerable<T> other)
    {
        if (Count == 0)
            return false;

        foreach (var item in other) {
            if (Contains(item))
                return true;
        }
        return false;
    }

    /// <summary>
    /// a == b<br/>
    /// Unlike BCL, this method doesn't check condition of except self
    /// </summary>
    public readonly bool SetEquals(IEnumerable<T> other)
    {
        if (Count == 0 && other.TryGetNonEnumeratedCount(out var otherCount) && otherCount > 0)
            return false;

        var bitArray = new StackAllocBitArray(
            stackalloc byte[StackAllocBitArray.GetArrayLength(_provider.Size)]);

        int found = 0;
        foreach (var key in other) {
            ref readonly var entry = ref _provider.GetItemRefOrNullRef(key);
            if (Unsafe.IsNullRef(in entry))
                return false;
            else {
                var index = _provider.GetInternalEntryIndex(in entry);
                if (!bitArray[index]) {
                    bitArray[index] = true;
                    found++;
                }
            }
        }

        return found == 0;
    }

    /// <summary>
    /// (a | b) - (a & b)
    /// </summary>
    /// <param name="other"></param>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        if (Count == 0)
            UnionWith(other);

        var bitArray = new StackAllocBitArray(
            stackalloc byte[StackAllocBitArray.GetArrayLength(_provider.Size)]);

        int found = 0;
        foreach (var key in other) {
            ref readonly var entry = ref _provider.GetItemRefOrNullRef(key);
            if (Unsafe.IsNullRef(in entry)) {
                Add(key);
            }
            else {
                var index = _provider.GetInternalEntryIndex(in entry);
                if (!bitArray[index]) {
                    bitArray[index] = true;
                    found++;
                }
            }
        }

        foreach (ref readonly var val in _provider) {
            if (bitArray[_provider.GetInternalEntryIndex(in val)])
                Remove(val);
        }
    }

    /// <summary>
    /// a |= b
    /// </summary>
    public void UnionWith(IEnumerable<T> other)
    {
        foreach (var item in other) {
            Add(item);
        }
    }

    void ICollection<T>.Add(T item) => Add(item);
    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator.Wrapper(_provider);
    readonly IEnumerator IEnumerable.GetEnumerator() => new Enumerator.Wrapper(_provider);

    #endregion

    public struct Enumerator
    {
        private AllocOptHashSetProvider<T, T, Comparer>.Enumerator _enumerator;

        internal Enumerator(in AllocOptHashSetProvider<T, T, Comparer> provider)
            => _enumerator = new(provider);

        public readonly T Current => _enumerator.Current;

        public bool MoveNext() => _enumerator.MoveNext();

        internal sealed class Wrapper(in AllocOptHashSetProvider<T, T, Comparer> provider) : IEnumerator<T>
        {
            private AllocOptHashSetProvider<T, T, Comparer>.Enumerator _enumerator = new(provider);

            public T Current => _enumerator.Current;

            object? IEnumerator.Current => Current;

            public void Dispose() { }
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator.Reset();
        }
    }

    internal readonly struct Comparer : IByKeyEqualityComparer<T, T>
    {
        private readonly TComparer _comparer;

        public bool Equals(T val, T key) => _comparer.Equals(val, key);

        public int GetHashCode([DisallowNull] T val) => _comparer.GetHashCode(val);
    }
}

// This is a wrapper of AOSet<T, WrappedEqualityComparer<TK>>
[CollectionBuilder(typeof(AllocOptCollectionBuilder), nameof(AllocOptCollectionBuilder.CreateSet))]
public struct AllocOptSet<T> : ISet<T>, IReadOnlySet<T>
{
    private AllocOptSet<T, WrappedEqualityComparer<T>> _set;

    public AllocOptSet()
        => _set = new(default);

    public AllocOptSet(int capacity)
        => _set = new(capacity, default);

    public AllocOptSet(IEqualityComparer<T> comparer)
        => _set = new(Unsafe.As<IEqualityComparer<T>, WrappedEqualityComparer<T>>(ref comparer));

    public AllocOptSet(int capacity, IEqualityComparer<T> comparer)
        => _set = new(capacity, Unsafe.As<IEqualityComparer<T>, WrappedEqualityComparer<T>>(ref comparer));

    #region Accessors

    public readonly int Count => _set.Count;

    public readonly int Capacity => _set.Capacity;

    public readonly bool Contains(T item) => _set.Contains(item);

    public readonly bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T value) => _set.TryGetValue(equalValue, out value);

    public readonly AllocOptSet<T, WrappedEqualityComparer<T>>.Enumerator GetEnumerator() => _set.GetEnumerator();

    #endregion

    #region Builders

    public bool Add(T item) => _set.Add(item);

    public bool Remove(T item) => _set.Remove(item);

    /// <remarks>
    /// This method won't clear elements in underlying array.
    /// Use <see cref="ClearUnreferenced"/> if you need it.
    /// </remarks>
    public void Clear() => _set.Clear();

    public void ClearUnreferenced() => _set.ClearUnreferenced();

    public void EnsureCapacity(int capacity) => _set.EnsureCapacity(capacity);

    #endregion

    #region Interface methods

    readonly bool ICollection<T>.IsReadOnly => false;

    public readonly void CopyTo(T[] array, int arrayIndex) => _set.CopyTo(array, arrayIndex);

    /// <summary>
    /// a -= b<br/>
    /// Unlike BCL, this method doesn't check condition of except self
    /// </summary>
    public void ExceptWith(IEnumerable<T> other) => _set.ExceptWith(other);

    /// <summary>
    /// a &= b<br/>
    /// Unlike BCL, this method doesn't check condition of except self
    /// </summary>
    public void IntersectWith(IEnumerable<T> other) => _set.IntersectWith(other);

    /// <summary>
    /// a &lt; b<br/>
    /// Unlike BCL, this method doesn't check condition of except self
    /// </summary>
    public readonly bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);

    /// <summary>
    /// a &gt; b<br/>
    /// Unlike BCL, this method doesn't check condition of except self
    /// </summary>
    public readonly bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);

    /// <summary>
    /// a &lt;= b<br/>
    /// Unlike BCL, this method doesn't check condition of except self
    /// </summary>
    public readonly bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);

    /// <summary>
    /// a &gt;= b<br/>
    /// Unlike BCL, this method doesn't check condition of except self
    /// </summary>
    public readonly bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);

    /// <summary>
    /// a & b &gt; 0<br/>
    /// Unlike BCL, this method doesn't check condition of except self
    /// </summary>
    public readonly bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);

    /// <summary>
    /// a == b<br/>
    /// Unlike BCL, this method doesn't check condition of except self
    /// </summary>
    public readonly bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);

    /// <summary>
    /// (a | b) - (a & b)
    /// </summary>
    /// <param name="other"></param>
    public void SymmetricExceptWith(IEnumerable<T> other) => _set.SymmetricExceptWith(other);

    /// <summary>
    /// a |= b
    /// </summary>
    public void UnionWith(IEnumerable<T> other) => _set.UnionWith(other);

    void ICollection<T>.Add(T item) => Add(item);
    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_set).GetEnumerator();
    readonly IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_set).GetEnumerator();

    #endregion
}
