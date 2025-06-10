# Trarizon.Library.Collections

Collections types, helpers for BCL collections and related types, more linqs

## Table of Contents

- [Collection types](#collection-types)
- [Collection helpers](#collection-helpers)
- [Other helpers](#helpers) : Helper for comparision, `Index`, `Range` ...
- [Linq](#linq)

## Collection types

`AllocOpt`| Collections
:-:|:--
`AllocOptList`| Value type list using pooled array as underlying array

`Buffers`| summary
:-:|:--
`IArrayAllocator`|Abstraction for array allocation and release

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

## Linq

Linq extensions, in class `TraEnumerable`

- Aggregation
    - `CountsMoreThan/LessThan/AtMost/AtLeast/EqualsTo/Between` : Judge size of collection
    - `IsDistinct(By)` : Check if the collection doesn't contains duplicate element
    - `IsInOrder(By)` : Check if the elements in collection is in order
    - `MinMax(By)` : Get minimun value and maximun value in one iteration
- Creation
    - `EnumerateByWhile/NotNull` : Yield next value selected by a `Func<T, T>`, until predicate failed
    - `EnumerateDescendantsDepth/BreadthFirst` : Traverse tree
- Element
    - `TryAt` : `TryXXX` version of `ElementAt`
    - `TryFirst` : `TryXXX` version of `First`
    - `TryLast` : `TryXXX` version of `Last`
    - `FirstNearToMax(By)(OrDefault)` : Find the first item has priority greater than given priority, if not found, return the first item with greatest priority
    - `TrySingle` : Returning tagged union version of `Single`
- Filtering
    - `Duplicates` : Return all elements that is not distinct in collection
    - `OfNotNull` : Filter out all `null` values
    - `TakeEvery` : Yield the values in specific interval
- Joining
    - `CatesianProduct` : Catesian product
    - `Interleave` : Alternates elements from each source.
    - `Merge` : Merge 2 sorted collections
- Mapping
    - `Adjacent` : Yield the value and its next value
    - `AggregateSelect` : `Aggregate` and returns all values in processing
    - `ChunkPair/Triple` : Returning tuple version of `Chunk`
    - `ChunkBy` : Similiar to `string.Split`
    - `WithIndex` : Yield return index and item, `Index` in .NET 9
    - `Intersperse` : Inserts the specified separator between each element of the source sequence.
    - `Repeat` : Repeatly enumerate the collection
    - `RepeatInterleave` : Repeat item in sequence
- Partition
    - `OfTypeWhile` : Take values until doesn't match the given type
    - `OfTypeUntil` : Take values until reach element in given type
    - `PopFront` : Split the collection into 2 parts, the first parts is return by `out` paramter
    - `PopFirst` : Get the first element, and returns the rest elements.
- Sorting
    - `LazyReverse` : Reverse, no cache if source is `IList<T>`
    - `Rotate` : Split the collection into 2 parts and swap them
- ToCollections
    - `ForEach`
    - `EmptyIfNull` : Return empty collection if source collection is `null`
    - `TryToNonEmptyList` : If collection is not empty, then collect items into `List<>`, in one iteration
