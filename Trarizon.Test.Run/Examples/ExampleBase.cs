namespace Trarizon.Library.RunTest.Examples;
internal abstract class ExampleBase : IDisposable
{
    private readonly string _name;
    private readonly string? _description;

    protected ExampleBase(string name, string? description = null)
    {
        _name = name;
        _description = description;
        Start();
    }

    protected ExampleBase(string? description = null)
    {
        _name = GetType().Name[..^"Example".Length];
        _description = description ?? "Press enter to start.";
        Start();
    }

    private void Start()
    {
        Console.WriteLine($"""
            --- Running Example [{_name}]
            --- {_description}

            """);
    }

    public void Dispose()
    {
        Console.WriteLine($"""

            --- End Example [{_name}]
            """);
    }
}
