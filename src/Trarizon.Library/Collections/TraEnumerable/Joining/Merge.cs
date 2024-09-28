namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    public static IEnumerable<T> Merge<T>(this IEnumerable<T> first, IEnumerable<T> second, IComparer<T>? comparer = default, bool descending = false)
    {
        if (first.IsEmptyArray())
            return second;
        else if (second.IsEmptyArray())
            return first;
        else
            return Iterate();

        IEnumerable<T> Iterate()
        {
            using var enumerator = first.GetEnumerator();
            using var enumerator2 = second.GetEnumerator();

            switch (enumerator.MoveNext(), enumerator2.MoveNext()) {
                case (true, true):
                    goto CompareAndSetNext;
                case (false, true):
                    goto IterSecondOnly;
                case (true, false):
                    goto IterFirstOnly;
                case (false, false):
                    yield break;
            }

        CompareAndSetNext:
            var left = enumerator.Current;
            var right = enumerator2.Current;
            comparer ??= Comparer<T>.Default;
            var compareRes = comparer.Compare(left, right);
            if (descending ? compareRes >= 0 : compareRes <= 0) {
                yield return left;
                if (enumerator.MoveNext())
                    enumerator.MoveNext();
                else
                    goto IterSecondOnly;
            }
            else {
                yield return right;
                if (enumerator2.MoveNext())
                    goto CompareAndSetNext;
                else
                    goto IterFirstOnly;
            }

        IterFirstOnly:
            do {
                yield return enumerator.Current;
            }
            while (enumerator.MoveNext());
            yield break;

        IterSecondOnly:
            do {
                yield return enumerator2.Current;
            } while (enumerator2.MoveNext());
            yield break;
        }
    }
}
