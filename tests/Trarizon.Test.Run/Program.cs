#pragma warning disable TRAEXP

using System.Text.Json;
using Trarizon.Library.Collections;
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

Console.WriteLine((string)obj.b.d.a);