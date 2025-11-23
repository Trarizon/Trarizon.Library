# Trarizon.Library.Collections

More collections types, and helpers for BCL collections and related types

## Table of Contents

- [Collection types](#collection-types)
- [Collection helpers](#collection-helpers)
- [Other helpers](#helpers) : Helper for comparision, `Index`, `Range` ...

## Collection types

`AllocOpt`| Collections
:-:|:--
`AllocOptList`| Value type list using pooled array as underlying array

`Buffers`| summary
:-:|:--
`IArrayAllocator`|Abstraction for array allocation and release
`NativeArray`|Unmanaged array

`Generic` | General generic types
:-:|:--
`BinaryTree<>` | Binary Tree
`Deque<>` | Double-ended queue
`IBinaryTreeNode<>` <br/> `BinaryTreeNode`| Abstraction for binary tree node
`ILinkedListNode<>` <br/> `LinkedListNode`| Abstraction for linked list node
`ListDictionary<,>`| Dictionary search by linear
`Memento<>`| General memento collection, Ring queue with an active pointer
`PrefixTree<>`| General prefix tree
`PrefixTreeDictionary<,>`| Dictionary implemented with prefix tree
`RingQueue<>`| Ring queue, a queue with fixed max size
`SortedList<>`| List that automatically sort item when adding

`StackAlloc` | Ref struct types
:-:|:--
`(ReadOnly)ConcatSpan<>`| Concat 2 `(ReadOnly)Span<>`s
`(ReadOnly)ReversedSpan<>` | Reversed `(ReadOnly)Span<>`
`StackAllocBitArray`| Bit array, use `Span<uint>` as underlying array

## Collection Helpers

Static class with `Tra` prefix, 

`Algorithm`| Algorithm provider
:-:|:--
`LinearSearch`|
`InsertionSort`|
`BubbleSort`|
`LevenshteinDistance`|

`Span`|For `Span<>` and `ReadOnlySpan<>`
:-:|:--
`TryAt`|
`AsReversed`| Create `ReversedSpan` from `Span`
static `As(ReadOnly)Bytes`| Create `Span` from a `unmanaged` value
`MoveTo`| Move items on `fromIndex` to `toIndex`
`FindLower(Upper)BoundIndex`| Find lower/upper bound in a sorted span
`OffsetOf`| Get index of a `ref`
`LinearSearch`|
Overloads for BCL exts| `Contains`, `Find`, `FindIndex`

`Array`| For `T[]` and `ImmutableArray<>`
:-:|:--
`EmptyIfDefault`| Convert to empty array if `ImmutableArray<>` is default
`AsEnumerable(OrNull)`| Convert `ImmutableyArray<>` to `IEnumerable<>` avoiding boxing

`Collection`| For BCL collections like `List<>`, `Dictionary<,>`
:-:|:--
`Singleton(T)`| Return a struct list wrapper with only one element
`Dict<,>.AtRef`| Get Value ref from `Dictionary<,>` or `throw`
`IDict.GetOrAdd`|
`IDict.AddOrUpdate`|
`AsSpan(Stack<>)`| Get `ReversedSpan` of a `Stack<>`
`AsSpan(List<>)`| Equivalent to `CollectionsMarshal.AsSpan()`, for .NET standard
`List<>.AtRef`| Get value ref
overload `List<>.AddRange`| Overload with `ReadOnlySpan<>`
overloads | `RemoveAt` `RemoveRange` with `Index`/`Range`
`List<>.MoveTo`| see `TraSpan.MoveTo`
`List<>.ReplaceAll` | Replace items in list
`List<>.GetLookup`| Get lookup wrapper for search
`List<>.GetSortedModifier`| Get sorted modifier, with which modifying list keeps list elements in order

## Helpers

`Comparison`| Helpers for comparison
:-:|:--
`IComparer.Reverse`| Reverse comparer
static `CreateComparable`| Create `IComparable<>` by 
static `CreateEquatable`| Create `IEquatable<>`

`Index`| Helpers for `Index`/`Range` or `int` as index
:-:|:--
static `FlipNegative`| Flip negative `int` value, use for handle return value of `Search`es
`Index.GetCheckedOffset`|
`Range.GetStartAndEndOffset`|
`Range.GetCheckedStartAndEndOffset`|
static `ValidateSliceArgs`|
