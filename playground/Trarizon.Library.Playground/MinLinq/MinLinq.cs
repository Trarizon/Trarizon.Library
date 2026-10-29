using System.Security.Cryptography.X509Certificates;

namespace Trarizon.Library.Playground.MinLinq;

public static class MinLinq
{
    extension<T>(IEnumerable<T> source)
    {
        public TResult MAggregate<TAccumulate, TResult>(TAccumulate seed, Func<T, TAccumulate, TAccumulate> accumulate, Func<TAccumulate, TResult> selector)
            => selector(source.MAggregateWhile((seed, accumulate),
                static (x, a) => ((a.accumulate(x, a.seed), a.accumulate), true)).Value.seed);

        public TAccumulate MAggregate<TAccumulate>(TAccumulate seed, Func<T, TAccumulate, TAccumulate> accumulate)
            => source.MAggregate(seed, accumulate, x => x);

        public T MAggregate(Func<T, T, T> accumulate)
            => source.MSkip(1).MAggregate(source.MFirst(), accumulate);

        public T MFirst()
            => source.MAggregateWhile(default(T)!, static (x, s) => (x, false)) is var (value,changed) && changed ? value : throw new InvalidOperationException("Sequence was empty");

        public T? MFirstOrDefault(T? defaultValue = default)
            => source.MAggregateWhile(defaultValue, static (x, s) => (x, false)).Value;

        public IEnumerable<TResult> MSelect<TResult>(Func<T, TResult> selector)
            => source.MSelectMany(selector, static IEnumerable<TResult> (x, s) => [s(x)]);

        public IEnumerable<T> MSkip(int count)
            => source.MSelectMany(count, static IEnumerable<(T, int)> (x, s) => --s >= 0 ? [] : [(x, s)])
                .Select(x => x.Item1);

        public IEnumerable<T> MTake(int count)
            => source.MSelectMany(count, static IEnumerable<(T, int)> (x, s) => --s > 0 ? [(x, s)] : [])
                .Select(x => x.Item1);

        public IEnumerable<T> MWhere(Func<T, bool> predicate)
            => source.MSelectMany(predicate, static IEnumerable<T> (x, s) => s(x) ? [x] : []);
    }
}