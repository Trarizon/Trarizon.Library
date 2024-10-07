# Trarizon.Library

Miscellaneous Helpers
This library use nuget package [`CommunityToolkit.HighPerformance`](https://github.com/communitytoolkit/dotnet) and `CommunityToolkit.Diagnostics` 

Contents:

- [CodeAnalysis](#CodeAnalysis)
- [CodeGeneration](#CodeGeneration)
- [Collections and helpers](#Collections)
- [Monads and wrappers](#Wrappers)
- [Other helpers](#More)

## CodeAnalysis

- `[BackingFieldAccess]` : a workround of .NET 9 backing field
- `[FriendAccess]` : Opt-in ver of `friend` in c++

## CodeGeneration

- `[Singleton]` : Generate a singleton class, thread safe with static field

## Collections

- namespace `AllocOpt` : Rewrite some collections with struct
- namespace `Generic` : Extension of `System.Collections.Generic`
    - `Deque<>` : Double-ended queue
    - `ListDictionary<,>` : Generic version of `System.Collections.Specialized.ListDictionary`.
    - `RingQueue<>` : Ring queue with fixed capacity, optional throw or overwrite when full
- namespace `StackAlloc` : `ref struct` collections
    - `ReadOnlyConcatSpan` : Concat 2 spans
    - `(ReadOnly)ReversedSpan` : Reversed span
    - `StackAllocBitArray` : Bit array using a `Span<byte>` as underlying items
- `TraXXX` : Static classes with helper method, extension methods
    - `TraAlgorithm` : 
    - `TraArray` : Helper for `T[]`, `ImmutableArray<>`
    - `TraComparison` : Helpers for `IComaprer<>`, `IComparable`, etc.
    - `TraDictionary` : Helper for `Dictionary<,>`
    - `TraEnumerable` : Helper for `IEnumerable`, linq extensions
    - `TraIter` : Value type simple Linq, this class is design for instant iterating, so it doesn't support chain linq
    - `TraList` : Helpers for `List<>`, `I(ReadOnly)List<>`
    - `TraSpan` : Helpers for `(ReadOnly)Span<>`

### Helpers

<details>
<summary>Array</summary>

- `MoveTo` : Move item on `fromIndex` to `toIndex`
- `EmptyIfDefault` for `ImmutableArray<>` : Return empty array if source is `null` 
- `TryAt` for `ImmutableArray<>`

</details>

<details>
<summary>Comparison</summary>

- `Reverse` : Reverse a `IComparer<>`

</details>

<details>
<summary>Dictionary</summary>

- `GetOrAdd`

</details>

<details>
<summary>Enumerable</summary>

- Aggregation
    - `CountsMoreThan/LessThan/AtMost/AtLeast/EqualsTo/Between` : Judge size of collection
    - `IsDistinct(By)` : Check if the collection doesn't contains duplicate element
    - `IsInOrder(By)` : Check if the elements in collection is in order
    - `MinMax(By)` : Get minimun value and maximun value in one iteration
- Creation
    - `EnumerateByWhile/NotNull` : Yield next value selected by a `Func<T, T>`, until predicate failed
- Element
    - `TryAt` : `TryXXX` version of `ElementAt`
    - `TryFirst` : `TryXXX` version of `First`
    - `FirstByMaxPriorityOrDefault` : Find the first item has priority greater than given priority, if not found, return the first item with greatest priority
    - `TrySingle` : Returning tagged union version of `Single`
- Filtering
    - `Duplicates` : Return all elements that is not distinct in collection
    - `OfNotNull` : Filter out all `null` values
    - `TakeEvery` : Yield the values in specific interval
- Joining
    - `CatesianProduct` : Catesian product
    - `Merge` : Merge 2 sorted collections
- Mapping
    - `Adjacent` : Yield the value and its next value
    - `AggregateSelect` : `Aggregate` and returns all values in processing
    - `ChunkPair/Triple` : Returning tuple version of `Chunk`
    - `WithIndex` : Yield index and item, `Index` in .NET 9
    - `Repeat` : Repeatly enumerate the collection
- Partition
    - `OfTypeWhile` : Take values until doesn't match the given type
    - `OfTypeUntil` : Take values until reach element in given type
    - `PopFront` : Split the collection into 2 parts, the first parts is return by `out` paramter
    - `PopFirst` : Get the first element, and returns the rest elements.
- Sorting
    - `Rotate` : Split the collection into 2 parts and swap them
- ToCollections
    - `EmptyIfNull` : Return empty collection if source collection is `null`
    - `TryToNonEmptyList` : If collection is not empty, then collect items into `List<>`, in one iteration
    - `TryGetSpan` : Get the underlying span if possible (`T[]` or `List<>`)

</details>

<details>
<summary>Iter</summary>

These methods are implements for instant iteration, so all iterators are implements with `struct`,
but not implements `IEnumerable<>` or `IEnumerator`

Too lazy to implement all linqs, so I'll just implement what I have used.

All extensions methods identifiers are start with `Iter`

- Creation
    - `IterateByWhile/NotNull` : Yield next value selected by a `Func<T, T>`, until predicate failed
    - `Range` : Enumerate `int` from `start` to `end`(not include)
    - `RangeTo` : Iterate `int` from 0 to `count` with specific step
- Filtering
    - `OfNotNull`
    - `OfType`
- Joining
    - `Zip`
- Partition
    - `Take`
- Sorting
    - `Reverse` : `Reverse` in Linq will always cache values in collection, this won't do that because designing for instant iteration

</details>

<details>
<summary>List</summary>

- Modification
    - `RemoveAt/RemoveRange` : overload for `Index` and `Range`
    - `MoveTo` : Move item(s) on `fromIndex` to `toIndex`
- Views
    - `GetLookup` : Returns a view treating the list as a set
    - `GetKeyedLookup` : Returns a view treating the list as dictionary
    - `GetSortedModifier` : Returns a view through which modifying the list will keep elements in order.

</details>

<details>
<summary>Span</summary>

- Creation
    - `As(ReadOnly)Bytes` : Convert an `unmanaged` value into bytes
- Index
    - `OffsetOf` : Get the index of element by pointer substraction
    - `FindLower/UppderBoundIndex` : find the lower/upper bound in a sorted span
    - `LinearSearch(FromEnd)` : Linear search, similar to `BinarySearch`, returns `~index` when not found
- Modifications
    - `MoveTo` : Move item(s) on `fromIndex` to `toIndex`
    - `SortStably` : Perform stable sort with BCL-built-in `Sort`, and `StableSortComparer`
- Views
    - `AsReversed` : return `(ReadOnly)ReversedSpan` of the span

</details>

## Wrappers

- `Either<,>` : Monad either
- `LazyInitDisposable` : Wrapper for fully utilizing the `using` statement when lazy-init `IDisposable` objects.
- `Result<,>` : Monad Result, for smaller size, `TError` only supports reference type, and if `TError` is null, the result means success
- `Optional<>` : Monad Option

## More

The namespace structure is almost the same with `System.XXX`

- namespace `IO`
    - `TraPath` : Extends `System.IO.Path`
    - `TraStream` : Helpers for `System.IO.Stream`
- namespace `Threading`
    - `AsyncSemaphoreLock` : Async lock implemented with `SemaphoreSlim`
    - `InterlockedBoolean` : Provide `Interlocked` methods for `bool`, this maybe removed in .NET 9
    - `InterlockedBooleanLock` : A lock implemented with `InterlockedBoolean`
    - `TraAsync` : Helpers for async operation, `Task<>`, `ValueTask<>`, etc.
- `TraEnum` : Helpers for enum types
- `TraNumber` : Helpers for number types, in `System.Numerics`
- `TraRandom` : Helpers for `Random`
- `TraString` : Helpers for `string`, interpolated string handler
- `TraUnsafe` : Extends `System.Runtime.CompilerServices.Unsafe`

### Helpers

- ArrayPool
    - `Rent` : Overload for `ArrayPool<>.Rent`, returns a auto-return object avaible with `using` statement
- Path
    - `Contains/ReplaceInvalidFileNameChar` : Operations about invalid filename characters, with cached `SearchValues<char>`
    - `Combine` : Overload for `ReadOnlySpan<char>`
- Stream
    - `Read(Exactly)` : Read data into `unmanaged` span
    - `ReadExactlyIntoArray` : Read exactly data into an `unmanaged` array with specific length
    - `ReadWithInt32Prefix` : Read a `int` as array length, and do `ReadExactlyIntoArray`
- Async
    - `GetAwaiter` : Support `await` keyword for `ValueTask?`, `ValueTask<>?`
- Enum
    - `HasAnyFlag` : Check if a enum value has one of given flags.
- Number
    - `IncAnd(Try)Wrap` : Increment the number, if the result is greater than given `max`, then wrap it
    - `Normalize` : Linear normalize value into [0,1]
    - `Normalize(Unclamped)` : Linear normalize value into [0,1], but not clamped
- Random
    - `SelectWeight` : Weighted random
    - `NextSingle/Double` : Get a random float number in specific range
- String
    - `Interpolated` : provider a method for call extra constructors of `DefaultInterpolatedStringHandler`
- Unsafe
    - `AsReadOnly` : Perform `Unsafe.As` for `ref readonly` variables
