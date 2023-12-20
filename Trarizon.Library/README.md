# Trarizon.Library

�����Ӱ˵ļ�����

## Collections.Creators

���ϴ���

`Type`|Method|Remarks
:-:|:-:|:--
`ArrayCreator`|`Repeat`|���ֵ�������
`ListCreator`|`Repeat`|���ֵ���`List<>`

## Collections.Extensions

��չ����

`this`|Method|Remarks
:-:|:-:|:--
`T[]`<br/>`List<>`|`Fill`|��伯��
`Dictionary<,>`<br/>`IDictionary<,>`|`GetOrAdd`|��ȡ����ֵ��������Ӳ�����ֵ
||`AddOrUpdate`|Add or Update
`Span<>`<br/>`ReadOnlySpan<>`|`OffsetOf`|ͨ��ָ�����Ԫ��/��������±�ֵ
||`IndexOf`|�����˴�ָ���±�ֵ��ʼ���ҵĹ���
||`Reverse`|��תspan
`T[]`<br/>`List<>`<br/>`Span<>`|`SortStably`|ʹ������`Sort`ʵ�ֵ��ȶ�����
`T[,]`|`AsSpan`<br/>`AsReadOnlySpan`|����ά�����е�һ��תΪ`Span<>`/`ROS<>`

## Collections.Extensions.Query

��Linq����չ����

<details>
<summary>չ��</summary>

`Enumerable`|Remarks
:-:|:--
`Adjacent`|�������ڵ�����ֵ�����±�(0,1), (1,2), ...��
`AggregateSelect`|��LinQ��`Aggregate`������ִ����ÿһ��Ԫ�صĽ��
`ChunkPair`<br/>`ChunkTriple`|��LinQ��`Chunk`�����ؽ��Ϊ`ValueTuple`
`CountsMoreThan`<br/>`CountsLessThan`<br/>`CountsAtLeast`<br/>`CountsAtMost`<br/>`CountsEqualsTo`<br/>`CountsBetween`|�Ƚ����д�С����ѡout������С��ָ��ֵʱ���ص�ǰ���д�С
`EmptyIfNull`|����Ϊ`null`ʱ���ؿ����У����򷵻�����
`IsInOrder`<br/>`IsInOrderBy`|�ж������Ƿ�����
`Merge`|�ϲ�������������
`MinMax`<br/>`MinMaxBy`|һ�α������������е���Сֵ�����ֵ
`PopFront`<br/>`PopFirst`<br/>`PopFrontWhile`|ȡ����ͷָ��������Ԫ�أ�*���ӳټ���*����������ʣ�µ�Ԫ��
`Repeat`|�������ظ�
`Rotate`|��������ǰ����������
`StartsWith`|��չ�˴�ָ��λ�ÿ�ʼ�ж��ķ���
`TrySingle`<br/>`TrySingleOrNone`|�ж������Ƿ������1(<=1)��ֵ�������ظ�ֵ����ָ��Ĭ��ֵ��
`WhereSelect`|�ϲ���LinQ��`Where`��`Select`���Դ˿��������м�ֵ

*`ListQuery`���ַ���ΪLinQ��`EnumerableQuery`��`IList<>`��ʵ�֣�����`EnumerableQuery`���ж�Ӧ��֧��
��Щ����������Ҫ���ṩ`IList<>`��Ϊ����ֵ����������Ϊ`<name>List`��ͬ����ע����*

`List`|Remarks
:-:|:--
`AsList`|��������
`AtOrDefault`|���±��ȡֵ��Խ�緵��Ĭ��ֵ
`PopFrontList`<br/>`PopFirstList`<br/>`PopFrontWhileList`|
`RepeatList`|
`ReverseList`|
`RotateList`|
`TakeList`|

</details>

## Extensions

�����Ӱ˵���չ����

`this`|Method|Remarks
:-:|:-:|:--
`Task`<br/>`Task<>`<br/>`ValueTask`<br/>`ValueTask<>`<br/>`ValueTask?`|`Sync`|`GetAwaiter().GetResult()`
`ValueTask?`|`GetAwaiter`|Ϊ`ValueTask?`�ṩ`await`�﷨֧��
`Task<>`<br/>`ValueTask<>`|`Select`|Monad
IFloatNumber|`Remap`<br/>`RemapInto`|��ֵӳ�䵽��һ����Χ
`Random`|`SelectWeighted`|��Ȩ����������ؽ���±�
||`Shuffle`|�����б�
||`NextSingle`<br/>`NextDouble`|�ڷ�Χ�����һλ������

</details>

## Wrappers

`Type`|Remarks
:-:|:--
`Optional<>`|Optional
`LazyInit<>`<br/>`LazyInit<,>`|һ���������ӳٳ�ʼ��

<details>
<summary><code>Optional&lt;&gt;</code>С����</summary>

`Optional<>` ʵ����`Deconstruct(out bool, out T)`��
��ˣ�����ʹ�����·�ʽ�����жϲ���ȡ`Value`
``` csharp
if (optional is (true, var value)) {
    Process(value);
}

_ = optional is (true, var value)
    ? value
    : default;
```

</details>