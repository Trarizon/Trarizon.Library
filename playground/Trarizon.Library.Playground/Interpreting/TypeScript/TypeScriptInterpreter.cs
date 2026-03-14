using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trarizon.Library.Playground.Interpreting.TypeScript;

public sealed class TypeScriptInterpreter : IDisposable
{
    private V8ScriptEngine _engine;

    public TypeScriptInterpreter()
    {
        _engine = new V8ScriptEngine();
    }

    public void Execute(string js)
    {
        _engine.Execute(js);

        var damage = (float)_engine.Script.ItemProcessor.CalculateDamage(1);
        Console.WriteLine(damage);

        foreach (var item in _engine.Script.ItemProcessor.array) {
            Console.WriteLine(item);
        }
        foreach(var item in _engine.Script.ItemProcessor) {
            Console.WriteLine(item.Key + ": " + item.Value);
        }
    }

    public void Dispose() => _engine.Dispose();

    public const string ts = """
        // ItemScript.ts
        var ItemProcessor = {
            name: "Magic Sword",
            baseDamage: 50,
            // 这是一个你需要在 C# 中调用的方法
            CalculateDamage: (level) => {
                return 50 + (level * 1.5);
            },
            GetPath: (category) => {
                return `assets/${category}/sword.png`;
            },
            array: [1, 2, 3]
        };
        """;
}
