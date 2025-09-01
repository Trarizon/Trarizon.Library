#pragma warning disable TRAEXP

using Microsoft.CodeAnalysis.CSharp;
using BenchmarkDotNet.Filters;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Generic;
using Trarizon.Library.Mathematics;
using Trarizon.Test.Run;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Runtime.CompilerServices;
using Trarizon.Library.Functional;
using System.Threading.Tasks;
using Trarizon.Library;


var res = await GetInfoAsync();

RunBenchmarks();

[AsyncMethodBuilder(typeof(InfoTaskBuilder<>))]
async Task<Info> GetInfoAsync()
{
    await Task.Delay(1000);
    return new Info();
}

class Info
{

}

static class Ext
{
    public static async Task<Result<Info, MyException>> ToResult(this Task<Info> info)
    {
        try {
            return await info.ConfigureAwait(false);
        }
        catch (MyException ex) {
            return ex;
        }
    }
}

struct Suppressed<T>
{
    public struct Awaiter : INotifyCompletion
    {
        private TaskAwaiter _value;
        public Awaiter(Task<T> value) => _value = value;
        public bool IsCompleted => _value.IsCompleted;
        public T GetResult() => _value.GetResult();
        public void OnCompleted(Action continuation) => _value.OnCompleted(continuation);
    }
}

struct InfoTaskBuilder<T>
{
    public Task<T> Task { get; }

    public static InfoTaskBuilder<T> Create() => new InfoTaskBuilder<T>();

    public void SetException(Exception exception) { }

    public void SetResult(T result) { }

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine { }

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine { }

    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine { }

    public void SetStateMachine(IAsyncStateMachine stateMachine) { }
}

public class MyException : Exception
{
    public MyException() { }
    public MyException(string message) : base(message) { }
    public MyException(string message, Exception inner) : base(message, inner) { }
    protected MyException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}