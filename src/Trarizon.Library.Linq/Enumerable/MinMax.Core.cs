using System.Numerics;

namespace Trarizon.Library.Linq;

partial class TraEnumerable
{
    private static bool TryMinMaxImpl<T, TRunner>(IEnumerable<T> source, ref TRunner runner)
        where TRunner : IMinMaxRunner<T, T>
        => TryMinMaxImpl<T, T, TRunner>(source, ref runner);

    private static bool TryMinMaxImpl<T, TResult, TRunner>(IEnumerable<T> source, ref TRunner runner)
        where TRunner : IMinMaxRunner<T, TResult>
    {
        if (source.TryGetSpan(out var span))
        {
            if (span.Length == 0)
            {
                return false;
            }
            runner.First(span[0]);
            for (var i = 1; i < span.Length; i++)
            {
                runner.Replace(span[i]);
            }
            return true;
        }

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return false;
        }
        runner.First(enumerator.Current);
        while (enumerator.MoveNext())
        {
            runner.Replace(enumerator.Current);
        }
        return true;
    }

    private interface IMinMaxRunner<T, TResult>
    {
        void First(T element);
        void Replace(T element);
    }

    private struct MinRunner<T, TResult, TTransformer, TSorter>(TTransformer transformer, TSorter sorter) : IMinMaxRunner<T, TResult>
        where TTransformer : ITransformer<T, TResult>
        where TSorter : IOrderComparer<TResult>
    {
        public TResult Value = default!;

        public void First(T element) => Value = transformer.Transform(element);
        public void Replace(T element)
        {
            var key = transformer.Transform(element);
            if (sorter.Less(key, Value))
                Value = key;
        }
    }

    private struct MinMaxRunner<T, TKey, TTransformer, TSorter>(TTransformer transformer, TSorter sorter) : IMinMaxRunner<T, (TKey Min, TKey Max)>
        where TTransformer : ITransformer<T, TKey>
        where TSorter : IOrderComparer<TKey>
    {
        public (TKey Min, TKey Max) Value = default!;

        public void First(T element) => Value.Min = Value.Max = transformer.Transform(element);
        public void Replace(T element)
        {
            var key = transformer.Transform(element);
            if (sorter.Less(key, Value.Min))
                Value.Min = key;
            if (sorter.Greater(key, Value.Max))
                Value.Max = key;
        }
    }

    private struct MinMaxByRunner<T, TKey, TTransformer, TSorter>(TTransformer transformer, TSorter sorter) : IMinMaxRunner<T, (T Min, T Max)>
        where TTransformer : ITransformer<T, TKey>
        where TSorter : IOrderComparer<TKey>
    {
        public (T Min, T Max) Value = default!;
        private (TKey Min, TKey Max) _key = default!;

        public void First(T element)
        {
            Value.Min = Value.Max = element;
            _key.Min = _key.Max = transformer.Transform(element);
        }
        public void Replace(T element)
        {
            var key = transformer.Transform(element);
            if (sorter.Less(key, _key.Min))
                (Value.Min, _key.Min) = (element, key);
            if (sorter.Greater(key, _key.Max))
                (Value.Max, _key.Max) = (element, key);
        }
    }

}
