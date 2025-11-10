namespace Trarizon.Library.Linq.Internal;

internal interface IChildrenEnumeratorProvider<T>
{
    IEnumerator<T> GetChildrenEnumerator(T source);
}
