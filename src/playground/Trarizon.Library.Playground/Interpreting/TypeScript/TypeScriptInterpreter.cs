using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trarizon.Library.Playground.Interpreting.TypeScript;

public sealed class TypeScriptInterpreter:IDisposable
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
    }

    public void Dispose() => _engine.Dispose();

    const string ts = """
        // ItemScript.ts
        export const ItemProcessor = {
            name: "Magic Sword",
            baseDamage: 50,
            // 这是一个你需要在 C# 中调用的方法
            CalculateDamage: (level: number) => {
                return 50 + (level * 1.5);
            },
            GetPath: (category: string) => {
                return `assets/${category}/sword.png`;
            }
        };
        """;
}
