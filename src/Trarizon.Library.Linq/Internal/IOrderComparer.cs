namespace Trarizon.Library.Linq.Internal;

internal interface IOrderComparer<T>
{
    bool Less(T x, T y);
    bool LessOrEqual(T x, T y);
    bool Greater(T x, T y);
    bool GreaterOrEqual(T x, T y);
}
