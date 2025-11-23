using System.Text.Json;
using Trarizon.Library.Text.Json.Dynamic;

namespace Trarizon.Library.Text.Json;

public static class TraJson
{
    public static WeakJsonElement GetWeakRootElement(this JsonDocument document) => new(document.RootElement);

    public static dynamic GetDynamicRootElement(this JsonDocument document, bool suppressNull = false) => document.RootElement.AsDynamic(suppressNull);

    public static dynamic AsDynamic(this JsonElement element, bool suppressNull = false)
        => suppressNull ? new NullSupressedDynamicJsonElement(element) : new DynamicJsonElement(element);
}
