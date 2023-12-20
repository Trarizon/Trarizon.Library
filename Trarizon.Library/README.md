# Trarizon.Library

杂七杂八的集合体

## Collections.Creators

集合创建

`Type`|Method|Remarks
:-:|:-:|:--
`ArrayCreator`|`Repeat`|填充值后的数组
`ListCreator`|`Repeat`|填充值后的`List<>`

## Collections.Extensions

扩展方法

`this`|Method|Remarks
:-:|:-:|:--
`T[]`<br/>`List<>`|`Fill`|填充集合
`Dictionary<,>`<br/>`IDictionary<,>`|`GetOrAdd`|获取键的值，否则添加并返回值
||`AddOrUpdate`|Add or Update
`Span<>`<br/>`ReadOnlySpan<>`|`OffsetOf`|通过指针计算元素/子数组的下标值
||`IndexOf`|重载了从指定下标值开始查找的功能
||`Reverse`|反转span
`T[]`<br/>`List<>`<br/>`Span<>`|`SortStably`|使用内置`Sort`实现的稳定排序
`T[,]`|`AsSpan`<br/>`AsReadOnlySpan`|将二维数组中的一行转为`Span<>`/`ROS<>`

## Collections.Extensions.Query

类Linq的扩展方法

<details>
<summary>展开</summary>

`Enumerable`|Remarks
:-:|:--
`Adjacent`|返回相邻的两个值（按下标(0,1), (1,2), ...）
`AggregateSelect`|类LinQ的`Aggregate`，返回执行至每一个元素的结果
`ChunkPair`<br/>`ChunkTriple`|类LinQ的`Chunk`，返回结果为`ValueTuple`
`CountsMoreThan`<br/>`CountsLessThan`<br/>`CountsAtLeast`<br/>`CountsAtMost`<br/>`CountsEqualsTo`<br/>`CountsBetween`|比较序列大小，可选out参数在小于指定值时返回当前序列大小
`EmptyIfNull`|序列为`null`时返回空序列，否则返回自身
`IsInOrder`<br/>`IsInOrderBy`|判断序列是否有序
`Merge`|合并两个有序序列
`MinMax`<br/>`MinMaxBy`|一次遍历返回序列中的最小值与最大值
`PopFront`<br/>`PopFirst`<br/>`PopFrontWhile`|取出开头指定数量的元素（*非延迟加载*），并返回剩下的元素
`Repeat`|将序列重复
`Rotate`|交换序列前后两个部分
`StartsWith`|扩展了从指定位置开始判定的方法
`TrySingle`<br/>`TrySingleOrNone`|判断序列是否仅含有1(<=1)个值，并返回该值（或指定默认值）
`WhereSelect`|合并了LinQ的`Where`和`Select`，以此可以利用中间值

*`ListQuery`部分方法为LinQ与`EnumerableQuery`的`IList<>`版实现，且在`EnumerableQuery`中有对应分支。
这些方法公开主要是提供`IList<>`作为返回值，命名规则为`<name>List`或同名，注释略*

`List`|Remarks
:-:|:--
`AsList`|返回自身
`AtOrDefault`|按下标获取值，越界返回默认值
`PopFrontList`<br/>`PopFirstList`<br/>`PopFrontWhileList`|
`RepeatList`|
`ReverseList`|
`RotateList`|
`TakeList`|

</details>

## Extensions

杂七杂八的扩展方法

`this`|Method|Remarks
:-:|:-:|:--
`Task`<br/>`Task<>`<br/>`ValueTask`<br/>`ValueTask<>`<br/>`ValueTask?`|`Sync`|`GetAwaiter().GetResult()`
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
`Optional<>`|Optional
`LazyInit<>`<br/>`LazyInit<,>`|一个轻量的延迟初始化

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