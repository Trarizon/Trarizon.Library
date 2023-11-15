using Trarizon.Yieliception.Yieliceptors;

namespace Trarizon.Yieliception;
internal interface IYieliceptableIterator<T>
{
    bool IsEnded { get; }
    IYieliceptor<T>? CurrentYieliceptor { get; }

    YieliceptionResult Next(T args);

    YieliceptionResult ForceNext(bool returnIfOccupied);
}

