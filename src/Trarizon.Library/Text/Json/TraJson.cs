using System.Text.Json;

namespace Trarizon.Library.Text.Json;
public static class TraJson
{
    public static WeakJsonElement GetWeakRootElement(this JsonDocument document) => new(document.RootElement);
}
