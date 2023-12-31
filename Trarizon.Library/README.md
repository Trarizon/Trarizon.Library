﻿# Trarizon.Library

Miscellaneous

Contents:

- [Collection extensions](#Collections)
- [Extensions for BCL type](#Extensions)
- [Simple wrapper types](#Wrappers)

## Collections

### Creators

static classes for quickly creating collections

- `ArrayCreator`
- `ListCreator`

### Extensions

扩展方法

`this`|Method|Remarks
--:|:--|:--
`T[]`<br/>`List<>`|`Fill`|Fill the collection with specific value
`Dictionary<,>`<br/>`IDictionary<,>`|`GetOrAdd`|获取键的值，否则添加并返回值
||`AddOrUpdate`|Add or Update
`Span<>`<br/>`ReadOnlySpan<>`|`OffsetOf`|通过指针计算元素/子数组的下标值
||`IndexOf`|重载了从指定下标值开始查找的功能
||`Reverse`|反转span
`T[]`<br/>`List<>`<br/>`Span<>`|`SortStably`|使用内置`Sort`实现的稳定排序
`T[,]`|`AsSpan`<br/>`AsReadOnlySpan`|将二维数组中的一行转为`Span<>`/`ROS<>`

LinQ-like extensions

<details>
<summary>展开</summary>

部分方法为多种集合进行了实现，下表中第一列指示实现类型，类型列表如下，不同类型的方法名添加了不同后缀以作区分
- `IList<>` : `List`
- `IReadOnlyList<>` : `ROList`

|Impl|Method|Remarks
|--:|:-:|:--|:-:
||`Adjacent`|返回相邻的两个值（按下标(0,1), (1,2), ...）
||`AggregateSelect`|类LinQ的`Aggregate`，返回执行至每一个元素的结果
||`ChunkPair`<br/>`ChunkTriple`|类LinQ的`Chunk`，返回结果为`ValueTuple`
||`CountsMoreThan`<br/>`CountsLessThan`<br/>`CountsAtLeast`<br/>`CountsAtMost`<br/>`CountsEqualsTo`<br/>`CountsBetween`|比较序列大小，可选out参数在小于指定值时返回当前序列大小
||`EmptyIfNull`|序列为`null`时返回空序列，否则返回自身
||`IsInOrder`<br/>`IsInOrderBy`|判断序列是否有序
||`Merge`|合并两个有序序列
||`MinMax`<br/>`MinMaxBy`|一次遍历返回序列中的最小值与最大值
|✔✔|`PopFront`<br/>`PopFirst`<br/>`PopFrontWhile`|取出开头指定数量的元素（*非延迟加载*），并返回剩下的元素
|✔✔|`Repeat`|将序列重复
|✔✔|`Reverse`|-
|✔✔|`Rotate`|交换序列前后两个部分
||`StartsWith`|扩展了从指定位置开始判定的方法
|✔✔|`Take`|-
||`TryFirst`|判断序列是否有值，若有，返回第一个值
||`TrySingle`<br/>`TrySingleOrNone`|判断序列是否仅含有1(<=1)个值，并返回该值（或指定默认值）
||`WhereSelect`|合并了LinQ的`Where`和`Select`，以此可以利用中间值

以下方法适用`IList<>`与`IReadOnlyList`

`List`|Remarks
:-:|:--
`AsList`|返回自身
`AtOrDefault`|按下标获取值，越界返回默认值

</details>

## Extensions

杂七杂八的扩展方法

`this`|Method|Remarks
:-:|:-:|:--
`Task`<br/>`Task<>`<br/>`ValueTask`<br/>`ValueTask<>`|`Sync`|`GetAwaiter().GetResult()`
`ValueTask?`|`GetAwaiter`|为`ValueTask?`提供`await`语法支持
`Task<>`<br/>`ValueTask<>`|`Select`|Monad
IFloatNumber|`Remap`<br/>`RemapInto`|将值映射到另一个范围
`Random`|`SelectWeighted`|按权重随机，返回结果下标
||`Shuffle`|打乱列表
||`NextSingle`<br/>`NextDouble`|在范围内随机一位浮点数

</details>

## Wrappers

`Type`|Remarks
:-:|:--
`LazyInit<>`<br/>`LazyInit<,>`|Lightweight `Lazy<>`
`Optional<>`|Optional

<details>
<summary><code>Optional&lt;&gt;</code>小寄巧</summary>

`Optional<>` 实现了`Deconstruct(out bool, out T)`，
因此，可以使用以下方式快速判断并获取`Value`
``` csharp
if (optional is (true, var value)) {
    Process(value);
}

_ = optional is (true, var value)
    ? value
    : default;
```

</details>