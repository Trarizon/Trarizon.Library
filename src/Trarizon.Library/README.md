# Trarizon.Library

Miscellaneous

Contents:

- [Code analysis by source generator](#CodeAnalysis)
- [Collection extensions](#Collections)
- [Extensions for BCL type](#Helpers)
- [Simple wrapper types](#Wrappers)

## CodeAnalysis

Analyers are in [Trarizon.Library.SourceGenerator](../Trarizon.Library.SourceGenerator)

Attribute|Analyzer|Remark
:-:|:-:|:--
`FriendAccess`|`FriendAccessAnalyzer`| friend in c++，限制可访问该成员的类
`BackingField`|`BackingFieldAnalyzer`| 代替backing field，限制可访问该字段的成员

## CodeTemplating

Generators are in [Trarizon.Library.SourceGenerator](../Trarizon.Library.SourceGenerator)

Attribute|Generator|Remark
:-:|:-:|:--
`Singleton`|`SingletonGenerator`| Generate singleton template
`UnionTag`<br/>`TagVariant`|`TaggedUnionGenerator`|Generated tagged union struct by enum

## Collections

### CollectionTypes

Rewrite BCL collections in struct, to reduce alloc on heap

- AllocOpt
    - `AllocOptDictionary<,>` <- `Dictionary<,>`
    - `AllocOptList<>` <- `List<>`
    - `AllocOptQueue<>` <- `Queue<>`
    - `AllocOptSet<>` <- `HashSet<>`
    - `AllocOptStack<>` <- `Stack<>`
- StackAlloc
    - `(Reversed)ReadOnlyRingSpan` - (or queue span?
    - `Reversed(ReadOnly)Span` - Use ext method `Span<>.Reverse()`
    - `StackAllocBitArray`
- Generic
    - `Deque`

### Helpers

static classes for BCL collections

- ArrayHelper : for `Array`, with `ImmutableArray<>`
- EnumerableHelper : for `IEnumerable<>`
- ListHelper : for `IList<>`, `IReadOnlyList<>`, with `List<>`
- SpanHelper : for `Span<>`, `ReadOnlySpan<>`

Types:
- ❌ : Not implemented
- ⭕ : Rounded method exists
- ✔ : Implicit implemented / BCL implemented
- 🟢 : Directly implemenented

#### Array/List/Span

Types|Method|Remarks
:--|:--|:--
||***Actions***
|🟢🟢✔|`Fill`|Fill the collection with specific value
|🟢🟢🟢|`SortStably`|使用内置`Sort`实现的稳定排序
||***Aggregation***
|⭕⭕🟢|`Sum`|
||***Creation***
|❌❌🟢|`AsBytes`|
||***Element***
|⭕🟢⭕|`AtRef`|获取`List<>`下标的值的引用
|✔✔🟢|`TryAt`|
|⭕⭕🟢|`IndexOf`|为Span的方法重载了从指定下标值开始查找的功能
|🟢⭕🟢|`OffsetOf`|通过指针计算元素/子数组的下标值
||***Sorting***
|✔✔🟢|`Reverse`|
||***Others***
|⭕⭕🟢|`AsReadOnly`|
|🟢⭕⭕<br/>`ImArr<>`|`EmptyIfNull`<br/>`EmptyIfDefault`|序列为`default`时返回空序列，否则返回自身

#### Enumerable/I(RO)List

Type|Method|Rename
:--|:--|:--
|🟢|`ForEach`|
||***Aggregation***
|🟢|`CountsMoreThan/LessThan`<br/>`CountsAtLeast/Most`<br/>`CountsEqualsTo`<br/>`CountsBetween`|比较序列大小，可选out参数在小于指定值时返回当前序列大小
|🟢|`IsDistinct(By)`|判断序列是否有重复元素
|🟢|`IsInOrder(By)`|判断序列是否有序
|🟢|`MinMax(By)`|一次遍历返回序列中的最小值与最大值
||***Creation***
|🟢|`EnumerateByWhile(NotNull)`|
||***Element***
|❌🟢|`TryAt`|以安全方式按下标获取值
|⭕🟢|`AtOrDefault`|以安全方式按下标获取值
|🟢|`FirstByMaxPriorityOrDefault`|获取第一个匹配Priority的值，若无则返回Priority最大的第一个值
|🟢|`TryFirst`|判断序列是否有值，若有，返回第一个值
|🟢|`TrySingle`|判断序列是否仅含有1个值（或为空），并返回该值或`default`
|🟢🟢|`StartsWith`|扩展了从指定位置开始判定的方法
||***Filtering***
|🟢|`OfNotNull`<br/>`OfNotNone`|`.Where(t is not null)` <br/> `.Where(t.HasValue)`
|🟢|`WhereSelect`|合并了LinQ的`Where`和`Select`，以此可以利用中间值
||***Joining***
|🟢🟢|`CartesianProduct`|返回两个序列的笛卡尔积（`SelectMany(_ => second, (_1, _2) => (_1, _2))`）
|🟢|`Merge`|合并两个有序序列
||***Mapping***
|🟢🟢|`Adjacent`|返回相邻的两个值（按下标(0,1), (1,2), ...）
|🟢|`AggregateSelect`|执行`Aggregate`，返回执行至每一个元素的结果
|🟢🟢|`ChunkPair/Triple`|类LinQ的`Chunk`，返回结果为`ValueTuple`
|🟢|`Index`|返回index和值的元组序列（.NET9有带）
|🟢🟢|`Repeat`|将序列重复
|🟢|`RepeatForever`|将序列重复
✔🟢|`Select(Cached)`|
||***Partition***
|🟢|`OfTypeUntil`|`.OfType<T>().TakeWhile(t is not TExcept)`
|🟢|`OfTypeWhile`|`.TakeWhile(t is T).OfType<T>()`
|🟢🟢|`PopFront(While)`<br/>`PopFirst`|取出开头指定数量的元素，并返回剩下的元素
|✔🟢|`Take`|
||***Sorting***
|✔🟢|`Reverse`|
|🟢🟢|`Rotate`|交换序列前后两个部分
||***Others***
|🟢🟢|`AsXXX`|返回自身
|❌🟢|`AsIListOrWrap`|如果实现了`IList`返回自身，否则wrap
|🟢|`EmptyIfNull`|序列为`null`时返回空序列，否则返回自身
|🟢|`ToListIfAny`|如果序列为空，返回`null`，否则等效`ToList`。作为优化方法避免`Any`的遍历

#### Dictionary

Type|Method|Rename
:--|:--|:--
||`GetOrAdd`|获取键的值，否则添加并返回值

## Helpers

杂七杂八的扩展方法

`this`|Method|Remarks
:-:|:-:|:--
`Task`<br/>`Task<>`<br/>`ValueTask`<br/>`ValueTask<>`<br/>`ValueTask?`<br/>`ValueTask<>?`|`Sync`|`GetAwaiter().GetResult()`
`ValueTask?`<br/>`ValueTask<>?`|`GetAwaiter`|为`ValueTask?`提供`await`语法支持，泛型返回`Optional<>`
`Task<>`<br/>`ValueTask<>`|`Select`|Monad
IFloatNumber|`Remap`<br/>`RemapInto`|将值映射到另一个范围
`Random`|`SelectWeighted`|按权重随机，返回结果下标
||`Shuffle`|打乱列表
||`NextSingle`<br/>`NextDouble`|在范围内随机一位浮点数

</details>

## Wrappers

`Type`|Remarks
:-:|:--
`Either<,>`|Monad Either
`NotNull<>`|Monad Optional for notnull reference type
`Optional<>`|Monad Optional
`Result<,>`|Monad Result, `TError`支持任意引用类型

Monad统一提供了`SelectXXX()`转换，`TryGetXXX()`获取值，以及一个非泛型静态类用于创建，等效`new()`。
Monad之间提供了`ToXXX()`和`AsXXX()`相互转换


<details>
<summary>小寄巧</summary>

可以使用以下方式快速判断Monad并获取`Value`
``` csharp
if (optional.TryGetValue(out var value)) {
    Process(value);
}

// 第二个参数可省略
if (result.TryGetValue(out var val, out var err) {
    Process(val);
}
else {
    Process(err);
}
```

</details>