#pragma warning disable TRAEXP

using System.Text.Json;
using Trarizon.Library.Linq;
using Trarizon.Library.Text.Json;

var json = JsonDocument.Parse("""
    {
        "a": [
            {
                "a": 1,
                "b": 2
            },
            {
                "a": 3,
                "b": 4
            }
        ],
        "b": {
            "b": 3,
            "c": 2
        }
    }
    """);

dynamic obj = json.GetDynamicRootElement(true);

//Console.WriteLine((string)obj.b.d.a);

IEnumerable<string> value(string v)
{
    if (v.Length == 0) {
        yield break;
    }
    else if (v.Length is 1) {
        yield break;
    }
    else {
        yield return v[..(v.Length / 2)];
        yield return v[(v.Length / 2)..];
    }
}

TraTraversable.Create("string", value)
    .TraverseDeepFirst()
    .ForEach(v => Console.WriteLine(v.Value));
