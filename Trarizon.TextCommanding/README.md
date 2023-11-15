# TextCommanding

> Input args format: 
> - param: `-p <value>`, `--prm <value>`
> - value: `val`, `"escaped ""value"" with space"`
> - Escape: `"` -> `""`

解析命令行输入并转为类

*TODO: 解析后直接执行指定命令*

<details>
<summary>类</summary>

Attributes|Remarks
:-:|:--
`TextCommand`|标记一个类为command
`TCConstructor`|标记为构造函数，解析时会调用这个构造函数
`TCOption`|标记字段/属性/构造函数参数为可选参数
`TCValue`|标记...为值，即无需指定形参名的参数，**限制类型为string或带有Parse(string[, IFormatProvider])函数的类型**~~~等我有心情了写converter~~~
`TCValues`|标记...为值集合，**限制类型为T[], IEnumerable<>, IReadOnlyList<>, IList<>, List<>，按该顺序判断类型**。**集合元素类型符合`[TCValue]`条件**

Types|Remarks
:-:|:--
`TCExecution`|供调用方使用的静态类
`ITextCommand`|*TODO*
`ParseArgsSettings`|`TCExecution.ParseArgs()`用参数
`TextCommandUtility`|公开了部分可使用方法

</details>

## 使用说明

调用`TCExecution.ParseArgs`将输入解析至自定义Args类。（就跟Deserialize差不多.jpg

*TODO:调用`TCExecution.Execute`解析输入并执行指定命令*

## 以防我忘了代码怎么跑的

输入解析过程：

1. `input:string` -> `ROM<char>` -> `IRawInput(StringRawInput)`
2. (`IRawInput`, `TextCommand`) -> `RawArguments` -> `T`

- `IRawInput` : 提供分割后的输入字符串
- `TextCommand` : 命令的定义
- `RawArguments` : 将输入整理为`Options`, `Flags`, `Values`。