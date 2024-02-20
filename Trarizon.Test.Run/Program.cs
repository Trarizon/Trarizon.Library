// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Creators;
using Trarizon.Library.Collections.Extensions;
using Trarizon.Library.Learn.SourceGenerator;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;
using Trarizon.Test.UnitTest;


IFilter<int> filter = default!;

filter.Predicate([
    default!,
    default!,
])
    .Predicate(default(Func<int, FilterResult>)!);

List<int> list;


static class FilterCreate
{
    public static FilterResult Success(string? err = default) => new() { Success = true, };

    public static FilterResult Failed(string error) => new() { Success = false, Errors = [error], };
}

readonly struct FilterResult
{
    public bool Success { get; init; }
    public string[] Errors { get; init; }
}

struct IFilter<T>
{
    Optional<T> value;
    public readonly T Value => value.Value;

    public readonly bool Success => value.HasValue;

    public IEnumerable<string> Errors { get; private set; }

    public IFilter<T> Predicate(ReadOnlySpan<Func<T, FilterResult>> filters)
    {
        foreach (var filter in filters) {
            Combine(filter(Value));
        }
        return this;
    }

    public IFilter<T> Predicate(Func<T, FilterResult> filter)
    {
        Combine(filter(Value));
        return this;
    }

    public IFilter<TResult> Select<TResult>(Func<T, TResult> selector)
    {
        if (Success)
            return new() { value = selector(Value), Errors = Errors };
        else
            return new() { Errors = Errors };
    }


    private void Combine(FilterResult filter)
    {
        if (!filter.Success && value.HasValue)
            value = default;
        Errors = Errors.Concat(filter.Errors.ToArray());
    }
}