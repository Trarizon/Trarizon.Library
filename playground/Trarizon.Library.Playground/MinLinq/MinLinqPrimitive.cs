using Trarizon.Library.Functional;

namespace Trarizon.Library.Playground.MinLinq;

public static class MinLinqPrimitive
{
    extension<T>(IEnumerable<T> source)
    {
        public IEnumerable<TResult> MSelectMany<TState, TResult>(TState state, Func<T, TState, IEnumerable<TResult>> selector)
        {
            foreach (var item in source)
            {
                foreach (var result in selector(item, state))
                {
                    yield return result;
                }
            }
        }

        public (TAccumulate Value,bool Changed) MAggregateWhile<TAccumulate>(
            TAccumulate seed,
            Func<T, TAccumulate, (TAccumulate Value, bool Next)> accumulate)
        {
            var current = seed;
            bool changed = false;
            foreach (var item in source)
            {
                changed = true;
                var (v, next) = accumulate(item, current);
                current = v;
                if (!next)
                    break;
            }
            return (current, changed);
        }
    }
}
