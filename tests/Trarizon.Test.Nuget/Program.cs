// See https://aka.ms/new-console-template for more information
using Trarizon.Library;
using Trarizon.Library.CodeGeneration;

Console.WriteLine("Hello, World!");
StringComparison comparison = default!;
comparison.HasAnyFlag(StringComparison.OrdinalIgnoreCase);

[Singleton]
partial class Proj
{

}