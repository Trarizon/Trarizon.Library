# History

## v1.1.0

- Remove some overload of `Random.NextItem`

## v1.0.1

- Seperate `Collections` to a new project
- [Break] Rename `TraNumber.CheckSliceArgs` to `Validate..`
- [Break] Rename `ILinkNode` to `ILinkedListNode`
- [Break] Rename namespace `Numerics` to `Mathematics`
- [Break] Move `SortedList`, `Memento`, `AutoAllocList` to `Specialized`
- [Break] Combine `TraCollection`, `TraList`, `TraDictionary` into `TraCollection`
- [Break] Remove `GetCheckedOffsetAndLength`, `Range.GetOffsetAndLength` checked it orz
- [Break] Remove `AllocOpt`
- [Break] Remove `ToKeyValuePair`
- [Break] Move `TraArrayPool` to `IArrayAllocatorExt`
- Change `IObjectAllocator.AutoReleaseScope` to generic
- Provide generic comparer to `List.Lookup`
- Add `IArrayAllocator`
- Add `ArrayFiller`, `ArrayTruncation`
- Add `SortedList<T>`,  `ContiguousLinkedList<>`
- Replace `Trie<>` with `PrefixTree<>`, add `PrefixTreeDictionary<,>`
- Add `EnumerateDescendants`
- Add `AutoAllocList` with generic Allocator
- Add `LazyReverse`, `LookAhead`
- Add `EventBus`
- Fix bugs on `Memento<T>`, remove generic restriction
- Support `ReversedSpan`, `Stack.AsSpan()` on all runtimes
- Support collection expression for `ListDictionary`
- Optimize `ChunkPair/Triple`

## v1.0.0

- Completely refactored

## v0.3.0 (in progress)

- Breaking
    - Add `GetValidValue` for monads
    - Monad.`Value`s will not throw if unset now
    - Moved `ReversedSpan`
    - Replace `ReversedSpan.OriginalSpan` with `Reverse()`
    - Rename `FriendAttribute` to `FriendAccessAttribute`, and moved to namespace `CodeAnalysis.MemberAccess`
    - Combine namespace `Collections.Creator` and `Collections.Extensions` to `Collections.Helpers`
    - Rename `Extension`s to `Helper`s
    - Unify parameter name in `AllocOptStack<>`
    - Optimize design of `TrySingle`
    - Rename `AllocOptCol.ClearUnreferenced` to `FreeUnreferenced`
    - Change `EnumerableQuery.PopFront`(lazy now), `MinMax`
    - Change `ListQuery.PopFront`
    - Redesigned `ListQuery`
    - Remove `Random.Shuffle`
    - Remove `IList.PopFrontWhile` `RepearForever`
    - Remove `T[,].AsSpan`
    - Remove `Array/List.Repeat()`
    - Combined `CollectionQuery` and `CollectionHelper`
    - Replace `Result<T,Exception>.ThrowIfFailed` with `GetValueOrThrow`
- New
    - Extend `ListHelper` methods:
        - `Adjacent()`
        - `Select()` indexed version
        - `CartesianProduct()`
        - `PopFront` overload
        - `TakeEvery`
    - Extend `EnumerableHelper` methods:
        - `OfTypeUntil()`
        - `Index()`
        - `OfNotNull()`
        - `OfXXX()` for wrappers
        - `FirstByMaxPriorityOrDefault()`
        - `PopFront` overload
        - `IsDistinct`, `IsDistinctBy`
        - `ToNonEmptyListOrNull`
        - `TakeEvery`
        - `WhereSelect` for wrappers
        - `CountsXXX` with predicate
        - `Range()`
    - Add `SpanHelper` methods:
        - `Sum`
    - Add `DictionaryHelper` methods:
        - `GetValueRef`
    - Extend collections:
        - `AllocOptQueue<T>`
        - `AllocOptDeque<>`
        - `Deque<>`
        - `RingQueue<>`
        - Add `PushRange` and multi-`Pop` to `AllocOptStack<T>`
        - Add a overload of `AllocOptDictionary<>.GetOrAddRef()`
        - Add `AllocOptCollectionBuilder.AsXXX()`
    - Extend helpers
        - Move `AsyncHelper` to namespace `Threading.Helpers`
        - Move `PathHelper` and `StreamHelper` to namespace `IO.Helpers`
        - Add `List<>.AtRef()`
        - Add `EnumerableHelper.EnumerateByWhile()`
        - Add `SpanHelper.As(ReadOnly)Bytes()`
        - Add `ValueTask<>?.GetAwaiter()`
        - Add `ValueTask<>?.Sync()`
        - Add `PathHelper` with `Combine`
        - Add `StreamHelper` with `Read<>()`
        - Add `StringHelper` with `Interpolated`
        - Add `UnsafeHelper` with `AsReadOnly`, public `Offset`
        - Add `LockHelper` with `InterlockedCompareExchange`/ `~Exchange` / `~Toggle`
        - Add `NumberHelper` with `IncMod`, `IncClamp`
    - Extend monads
        - `NotNull<T>`
        - `IOptional<>`
        - `IEither<>`
        - Add method `GetValueRefOrDefaultRef()`
        - Add more creator for `Optional<>`
    - Extend Generator/Analyzer
        - Add `BackingFieldAccessAttribute`
        - `FriendAccessAttribute` now works on pointer access
        - Add `FriendAccess.Options`
        - Add `SingletonAttribute`
    - Add `InterlockedBoolean`, `InterlockedBooleanLock`
- Changes
    - Optimize `IEnumerable<>.CartesianProduct()`, `IsInOrder()`, `Merge()`, `PopFront`
    - Optimize condition check in Queries
    - Implement `ICriticalNotifyCompletion` for `NullableValueTaskAwaiter<>`
    - Adjust method in c#, no breaking for caller
    - Optimize collection expression creation of `AllocOptStack/Queue`
    - Optimize internal impl in `AllocOpt`
    - Optimize `ValueTask?.GetAwaiter()` and `Sync()`
    - Change ref kind of wrapper.`GetValueRefOrDefaultRef`
- Bugs
    - Fix bug when use array type in `[FriendAccess]`, analyzer will crash. (though maybe nobody will do this
    - Fix bug on `AllocOptList.ClearUnreferenced()`
    - Fix bug on non-param ctor on `AllocOptSet/Dictionary`
    - Fix bug on `AllocOptDictionary.Add`
    - MemberAccess analyzers are now works on constructor

## v0.2.2

- New
    - Add more collection members to `StackAllocBitArray`
    - Add `ReversedSpan<>.CopyTo`
    - Add overload method `IEnumerable<>.StartWith()` for `T[]`
    - Add `IEnumerable<>.CartesianProduct`
    - Add `FriendAttribute`
    - public `StableSortComparer<>` and provide new overloads
- Changes
    - Optimize `AllocOptSet<>` with `StackAllocBitArray`
    - Optimize `IEnumerable<>.TrySingle()`, `TryFirst()`
    - Remove unused method in `IKeyedEqualityComparer`(internal)
    - Optimize internal structure of `AllocOptSet<>`

## v0.2.1

- New
    - Add `AllocOptDictionary<,>`, `AllocOptList<>`, `AllocOptSet<>`, `AllocOptStack<>`
    - Add `RepeatForever()`
    - Add `Either<,>.ToString()`
    - Add `ReversedSpanQuerier<>.Slice()` and `ToArray()`
    - Add `T[].OffsetOf()`
    - Add `IEnumerable<>.ForEach()`
    - Add `StackAllocBitArray`
- Breaking
    - Remove `ArrayFiller<>`, use `AllocOptList<>` instead
    - Adjust signature of `IEnumerable<>.Merge` and `IEnumerable.IsInOrder`
    - Rename `Span<>.Reverse` to `ToReversedSpan`
    - Move `Span<>.Reverse` to `SpanExtensions`
- Changes
    - Change format of `Optional<>.ToString()`, `Result<,>.ToString()`, and they won't return `null` now
    - Optimize nullable analysis for `IEnumerable<>.TrySingle()`, `TrySingleOrNone()`, `PopFirst()`
- Bug fix
    - Order judge in `IEnumerable<>.Merge`

## v0.2.0

- New
    - Add conversion between wrappers
    - Add `ArrayFiller<>`
    - Add `Either<,>`
    - Add `IList<>.TryAt()`，`IROList<>.TryAt()`
    - Add `Optional<>.Unwrap`
- Changes
    - Use Exception static methods in .NET 8 to replace part of `ThrowHelper`
- Breaking
    - Remove target framework .NET Standard 2.0 (~~反正源生成器不能用，不如等什么时候源生成器支持新版.NET吧.jpg~~
    - `Span.Reverse` is moved to `SpanQuery`
    - Remove `IList<>.AtOrDefault()`
    - Rename `IReadOnlyList<>.AtOrDefault()` to `ElementAtOrDefault`
    - Modified signature of `IEnumerable<>.Merge`
- Bug fix
    - Exception judge in `IEnumerable.CountsBetween`
