# Yieliception

> ~actually it means yielding interception��С��~
> 
> Intercept on Iterator.Resume()

> ~����������ⶫ���Ǹ���qq botд�ģ�����bot��ܱ��ɵ���һ��Ʊ��������������bot����.jpg�����������������~

������`IEnumerator<>`��`IAsyncEnumerator<>`ʵ�ֵ�״̬���ṩ��`MoveNext()`�����أ�
������`yield`�ڼ������ֵ�����жϻ򴫵�

����ʵ���˵��÷���`IEnumerator<>`��ͨ�ţ���python��`send()`

��Ҫ��Ϊ����չ`yield return`��ͨ���������⽻���Ա�����yieldд������ͨ��״̬��

<details>
<summary>Types</summary>

������������������������ʹ������ĸ��д����

Type|Remarks
:-:|:--
`YieliceptableIterator<>`<br/>`AsyncYieliceptableIterator<>`|�����صĵ���������ʹ��`YI<object>`��Ϊ�޲ΰ汾
`YieliceptionExtensions`|�ṩ��`ToYieliceptable`��`IEnumerator<IYieliceptor<>>`תΪ`YI<>`<br/>��������ط���
***Yieliceptors*** | ������
`IYieliceptor<>`|`yield return`����ֵ���ܽӿڡ�ʹ��`IY<object>`��Ϊ�޲ΰ汾���޲η�ʽ����ʱ������Ϊ`null`
`TimerYieliceptor`|�����أ����ǵ���ʱ���������
`ValidationYieliceptor<,>`<br/>`ValidationYieliceptor<>`<br/>`ConditionYieliceptor`|����ί����Ϊ�����ж�
`ValueDeliveryYieliceptor<>`|���ݶ�����
`ValueDeliverer<,>`|��Ϊ������ͨ�����
***Components*** | ���
`IYieliceptionComponent`<br/>`IAsyncYieliceptionComponent`|�ܽӿ�
`YieliceptionTimer`<br/>`ITimerYieliceptor`|��ʱ����һ��ʱ��δ��֤/�����غ��Զ���һ��������ʹ���̰߳�ȫ�汾

��ͬһ���������ڣ�`Yieliceptor`�������ڿ������࣬��ʹ�ù���`WithXXX()`������������

</details>

## Iterator Interception

> ����[`RunTest.Examples.YieliceptionExample`](../Trarizon.Test.Run/Examples/YieliceptionExample.cs)

��������������`IEnumerator<IYieliceptor<>?>`��ͨ��`yield return`�����������

ʹ��`IEnumerator<IYieliceptor<>?>.ToYieliceptable()`���������ص�������

### Yieliceptor

�Զ���������ʵ��`IYieliceptor`

��������resumeʱ����ͨ��`IYieliceptor<>.CanMoveNext()`�ж��Ƿ�resume��
ͬʱ����˷�������ʵ�ֵ��÷����������ֵ��

`yieliceptor`Ϊ`null`ʱ���˴ι������жϣ���ʼ�շ���true����

- `IY<>.CanMoveNext(T)`�ж��Ƿ���Լ���������`false`ʱ�ڲ�Enumerator�������`MoveNext()`��

### Component

�Զ������ʵ��`IYieliceptableComponent`��

- `IYC.OnNext(YI, bool)`��ÿ��`YI.Next(T)`��Enumeratorδ����ʱ���ã�

## ������ͨ��

> ����[`RunTest.Examples.YieliceptionExample.RunCommunication`](../Trarizon.Test.Run/Examples/YieliceptionExample.cs)

*Similar to Python*

Ϊ`IEnumerator<ValueDeliver<,>>`�ṩ����չ����`SendAndNext(T)`��
ͨ��`ValueDeliverer<,>`���е��÷����������ͨ��