# Yieliception

> ~actually it means yielding interception（小声~
> 
> Intercept on Iterator.Resume()

> ~碎碎念：本来这东西是给我qq bot写的，但是bot框架被干掉了一大票，于是由于怕死bot咕了.jpg。这个东西留了下来~

给基于`IEnumerator<>`和`IAsyncEnumerator<>`实现的状态机提供对`MoveNext()`的拦截，
可以在`yield`期间对输入值进行判断或传递

附带实现了调用方与`IEnumerator<>`的通信（见python的`send()`

主要是为了扩展`yield return`，通过允许内外交互以便于用yield写出可以通信状态机

<details>
<summary>Types</summary>

简便起见，下文中类名过长会使用首字母缩写代替

Type|Remarks
:-:|:--
`YieliceptableIterator<>`<br/>`AsyncYieliceptableIterator<>`|带拦截的迭代器，可使用`YI<object>`作为无参版本
`YieliceptionExtensions`|提供了`ToYieliceptable`将`IEnumerator<IYieliceptor<>>`转为`YI<>`<br/>与其他相关方法
***Yieliceptors*** | 拦截器
`IYieliceptor<>`|`yield return`返回值的总接口。使用`IY<object>`作为无参版本以无参方式调用时，参数为`null`
`TimerYieliceptor`|无拦截，但是倒计时结束会继续
`ValidationYieliceptor<,>`<br/>`ValidationYieliceptor<>`<br/>`ConditionYieliceptor`|传入委托作为拦截判断
`ValueDeliveryYieliceptor<>`|传递对象用
`ValueDeliverer<,>`|作为迭代器通信组件
***Components*** | 组件
`IYieliceptionComponent`<br/>`IAsyncYieliceptionComponent`|总接口
`YieliceptionTimer`<br/>`ITimerYieliceptor`|计时器，一段时间未验证/被拦截后自动下一步，建议使用线程安全版本

在同一个迭代器内，`Yieliceptor`基本属于可重用类，可使用公开`WithXXX()`方法进行重用

</details>

## Iterator Interception

> 例见[`RunTest.Examples.YieliceptionExample`](../Trarizon.Test.Run/Examples/YieliceptionExample.cs)

迭代器方法返回`IEnumerator<IYieliceptor<>?>`，通过`yield return`挂起迭代器。

使用`IEnumerator<IYieliceptor<>?>.ToYieliceptable()`创建可拦截迭代器。

### Yieliceptor

自定义拦截器实现`IYieliceptor`

当迭代器resume时，会通过`IYieliceptor<>.CanMoveNext()`判断是否resume。
同时，借此方法可以实现调用方向迭代器传值。

`yieliceptor`为`null`时，此次挂起不做判断（即始终返回true）。

- `IY<>.CanMoveNext(T)`判断是否可以继续，返回`false`时内部Enumerator不会调用`MoveNext()`；

### Component

自定义组件实现`IYieliceptableComponent`。

- `IYC.OnNext(YI, bool)`在每次`YI.Next(T)`且Enumerator未结束时调用；

## 迭代器通信

> 例见[`RunTest.Examples.YieliceptionExample.RunCommunication`](../Trarizon.Test.Run/Examples/YieliceptionExample.cs)

*Similar to Python*

为`IEnumerator<ValueDeliver<,>>`提供了扩展函数`SendAndNext(T)`。
通过`ValueDeliverer<,>`进行调用方与迭代器的通信