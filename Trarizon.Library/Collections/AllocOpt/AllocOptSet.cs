using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.AllocOpt.Providers;

namespace Trarizon.Library.Collections.AllocOpt;
// Unlike HashSet<>, this collection doesn't check collision count
public struct AllocOptSet<T, TComparer> : ISet<T>, IReadOnlySet<T>
    where TComparer : IEqualityComparer<T>
{
    private AllocOptDictionary<T, ValueTuple, TComparer> _dict;

    public AllocOptSet(TComparer comparer)
        => _dict = new(comparer);

    public AllocOptSet(int capacity, TComparer comparer)
        => _dict = new(capacity, comparer);

    #region Accessors

    public readonly int Count => _dict.Count;

    public readonly int Capacity => _dict.Capacity;

    public readonly bool Contains(T item) => _dict.ContainsKey(item);

    public readonly bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T value)
    {
        ref readonly var entry = ref _dict._provider.GetEntryRefOrNullRef(equalValue);
        if (Unsafe.IsNullRef(in entry)) {
            value = default;
            return false;
        }

        value = entry.Key;
        return true;
    }

    public readonly AllocOptDictionary<T, ValueTuple, TComparer>.KeyCollection.Enumerator GetEnumerator() => _dict.Keys.GetEnumerator();

    #endregion

    #region Builders

    public bool Add(T item) => _dict.TryAdd(item, default);

    public bool Remove(T item) => _dict.Remove(item);

    /// <remarks>
    /// This method won't clear elements in underlying array.
    /// Use <see cref="ClearUnreferenced"/> if you need it.
    /// </remarks>
    public void Clear() => _dict.Clear();

    public void ClearUnreferenced() => _dict.ClearUnreferenced();

    public void EnsureCapacity(int capacity) => _dict.EnsureCapacity(capacity);

    #endregion

    #region Interface methods

    readonly bool ICollection<T>.IsReadOnly => false;

    public readonly void CopyTo(T[] array, int arrayIndex)
        => _dict.Keys.CopyTo(array, arrayIndex);

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

        // TODO: BitArray Opt
        BitArray bitArray = new(_dict._provider.Size);

        foreach (var key in other) {
            ref readonly var entry = ref _dict._provider.GetEntryRefOrNullRef(key);
            if (!Unsafe.IsNullRef(in entry))
                bitArray[_dict._provider.GetInternalEntryIndex(in entry)] = true;
        }

        var enumerator = new AllocOptHashMapProvider<T, ValueTuple, TComparer>.Enumerator(_dict._provider);
        while (enumerator.MoveNext()) {
            ref readonly var entry = ref enumerator.Current;
            if (!bitArray[_dict._provider.GetInternalEntryIndex(in entry)])
                Remove(entry.Key);
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

        BitArray bitArray = new(_dict._provider.Size);

        int found = 0;
        bool isProper = false;
        foreach (var key in other) {
            ref readonly var entry = ref _dict._provider.GetEntryRefOrNullRef(key);
            // Found item not in other, result maybe proper
            if (Unsafe.IsNullRef(in entry))
                isProper = true;
            else {
                var index = _dict._provider.GetInternalEntryIndex(in entry);
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

        BitArray bitArray = new(_dict._provider.Size);

        int found = 0;
        foreach (var key in other) {
            ref readonly var entry = ref _dict._provider.GetEntryRefOrNullRef(key);
            // found item not in set, other is not subset of set
            if (Unsafe.IsNullRef(in entry))
                return false;
            else {
                var index = _dict._provider.GetInternalEntryIndex(in entry);
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

        BitArray bitArray = new(_dict._provider.Size);

        int found = 0;
        foreach (var key in other) {
            ref readonly var entry = ref _dict._provider.GetEntryRefOrNullRef(key);
            if (!Unsafe.IsNullRef(in entry)) {
                var index = _dict._provider.GetInternalEntryIndex(in entry);
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

        BitArray bitArray = new(_dict._provider.Size);

        int found = 0;
        foreach (var key in other) {
            ref readonly var entry = ref _dict._provider.GetEntryRefOrNullRef(key);
            if (Unsafe.IsNullRef(in entry))
                return false;
            else {
                var index = _dict._provider.GetInternalEntryIndex(in entry);
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

        BitArray bitArray = new(_dict._provider.Size);

        int found = 0;
        foreach (var key in other) {
            ref readonly var entry = ref _dict._provider.GetEntryRefOrNullRef(key);
            if (Unsafe.IsNullRef(in entry)) {
                Add(key);
            }
            else {
                var index = _dict._provider.GetInternalEntryIndex(in entry);
                if (!bitArray[index]) {
                    bitArray[index] = true;
                    found++;
                }
            }
        }

        var enumerator = new AllocOptHashMapProvider<T, ValueTuple, TComparer>.Enumerator(_dict._provider);
        while (enumerator.MoveNext()) {
            ref readonly var entry = ref enumerator.Current;
            if (bitArray[_dict._provider.GetInternalEntryIndex(in entry)])
                Remove(entry.Key);
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
    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_dict.Keys).GetEnumerator();
    readonly IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict.Keys).GetEnumerator();

    #endregion
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

    public readonly AllocOptDictionary<T, ValueTuple, WrappedEqualityComparer<T>>.KeyCollection.Enumerator GetEnumerator() => _set.GetEnumerator();

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
