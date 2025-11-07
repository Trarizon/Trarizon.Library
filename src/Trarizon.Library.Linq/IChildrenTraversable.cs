namespace Trarizon.Library.Linq;

public interface IChildrenTraversable<out T>
{
    IEnumerator<T> GetChildrenEnumerator();
}
