namespace Trarizon.Yieliception.Components;
public interface IYieliceptionComponent : IDisposable
{
    /// <param name="moved">
    /// <see langword="false"/> represents <see cref="YieliceptionResult.Rejected"/>,
    /// <see langword="true"/> represents <see cref="YieliceptionResult.Moved"/>
    /// </param>
    void OnNext<T>(YieliceptableIterator<T> iterator, bool moved);
}

public interface IAsyncYieliceptionComponent : IDisposable
{
    /// <param name="moved">
    /// <see langword="false"/> represents <see cref="YieliceptionResult.Rejected"/>,
    /// <see langword="true"/> represents <see cref="YieliceptionResult.Moved"/>
    /// </param>
    void OnNext<T>(AsyncYieliceptableIterator<T> iterator, bool moved);
}
