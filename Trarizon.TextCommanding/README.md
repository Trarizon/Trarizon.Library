# TextCommanding

> Input args format: 
> - param: `-p <value>`, `--prm <value>`
> - value: `val`, `"escaped ""value"" with space"`
> - Escape: `"` -> `""`

�������������벢תΪ��

*TODO: ������ֱ��ִ��ָ������*

<details>
<summary>��</summary>

Attributes|Remarks
:-:|:--
`TextCommand`|���һ����Ϊcommand
`TCConstructor`|���Ϊ���캯��������ʱ�����������캯��
`TCOption`|����ֶ�/����/���캯������Ϊ��ѡ����
`TCValue`|���...Ϊֵ��������ָ���β����Ĳ�����**��������Ϊstring�����Parse(string[, IFormatProvider])����������**~~~������������дconverter~~~
`TCValues`|���...Ϊֵ���ϣ�**��������ΪT[], IEnumerable<>, IReadOnlyList<>, IList<>, List<>������˳���ж�����**��**����Ԫ�����ͷ���`[TCValue]`����**

Types|Remarks
:-:|:--
`TCExecution`|�����÷�ʹ�õľ�̬��
`ITextCommand`|*TODO*
`ParseArgsSettings`|`TCExecution.ParseArgs()`�ò���
`TextCommandUtility`|�����˲��ֿ�ʹ�÷���

</details>

## ʹ��˵��

����`TCExecution.ParseArgs`������������Զ���Args�ࡣ���͸�Deserialize���.jpg

*TODO:����`TCExecution.Execute`�������벢ִ��ָ������*

## �Է������˴�����ô�ܵ�

����������̣�

1. `input:string` -> `ROM<char>` -> `IRawInput(StringRawInput)`
2. (`IRawInput`, `TextCommand`) -> `RawArguments` -> `T`

- `IRawInput` : �ṩ�ָ��������ַ���
- `TextCommand` : ����Ķ���
- `RawArguments` : ����������Ϊ`Options`, `Flags`, `Values`��