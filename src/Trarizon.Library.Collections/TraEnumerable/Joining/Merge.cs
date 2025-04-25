namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    public static IEnumerable<T> Merge<T>(this IEnumerable<T> first, IEnumerable<T> second)
        => Merge(first, second, Comparer<T>.Default);

    public static IEnumerable<T> Merge<T, TComparer>(this IEnumerable<T> first, IEnumerable<T> second, TComparer comparer) where TComparer : IComparer<T>
    {
        if (first.IsEmptyArray())
            return second;
        else if (second.IsEmptyArray())
            return first;
        else
            return Iterate(first, second, comparer);

        static IEnumerable<T> Iterate(IEnumerable<T> first, IEnumerable<T> second, TComparer comparer)
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
            if (comparer.Compare(left, right) <= 0) {
                yield return left;
                if (enumerator.MoveNext())
                    goto CompareAndSetNext;
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
            } while (enumerator.MoveNext());
            yield break;

        IterSecondOnly:
            do {
                yield return enumerator2.Current;
            } while (enumerator2.MoveNext());
            yield break;
        }
    }
}
