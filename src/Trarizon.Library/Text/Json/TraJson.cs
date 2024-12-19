// Add nuget System.Text.Json for .NET Standard is ok, but i wonder it is really neccessary to add a json lib in it
#if !NETSTANDARD2_0

using System.Text.Json;

namespace Trarizon.Library.Text.Json;
public static class TraJson
{
    public static WeakJsonElement GetWeakRootElement(this JsonDocument document) => new(document.RootElement);
}

#endif
