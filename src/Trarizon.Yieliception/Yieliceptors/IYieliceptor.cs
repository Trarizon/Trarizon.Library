namespace Trarizon.Yieliception.Yieliceptors;
public interface IYieliceptor<in TArgs>
{
    bool CanMoveNext(TArgs args);
}
