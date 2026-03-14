namespace Trarizon.Library.Experimental.Interpreting.Brainfuck;
public sealed class BrainfuckInterpreter
{
    public bool EnableRead { get; set; }

    public bool EnableWrite { get; set; }

    public void Interpret(ReadOnlySpan<char> expression)
        => Interpret(expression,
            EnableRead ? new BrainfuckTextReader(Console.In) : default,
            EnableWrite ? new BrainfuckTextWriter(Console.Out) : default);

    public void Interpret(ReadOnlySpan<char> expression, TextReader? reader, TextWriter? writer)
        => Interpret(expression, new BrainfuckTextReader(reader!), new BrainfuckTextWriter(writer!));

    public void Interpret<TReader, TWriter>(ReadOnlySpan<char> expression, TReader input, TWriter output)
        where TReader : IBrainfuckReader where TWriter : IBrainfuckWriter
    {
        var reader = new BrainfuckExpressionReader(expression, stackalloc int[64]);
        var memory = new BrainfuckMemory(new byte[1024]);

        while (reader.TryReadNext(out var ch)) {
            switch (ch) {
                case '>': memory.Pointer++; break;
                case '<': memory.Pointer--; break;
                case '+': memory.CurrentData++; break;
                case '-': memory.CurrentData--; break;
                case '.':
                    if (EnableWrite) { output.Write(memory.CurrentData); break; }
                    else goto default;
                case ',':
                    if (EnableRead) { memory.CurrentData = input.Read(); break; }
                    else goto default;
                case '[':
                    if (memory.CurrentData == 0) reader.ReadUntil(']');
                    else reader.EnterLoop();
                    break;
                case ']':
                    if (memory.CurrentData == 0) reader.ExitLoop();
                    else reader.Loop();
                    break;
                default:
                    throw new BrainfuckException("Invalid identifier");
            }
        }
    }
}
