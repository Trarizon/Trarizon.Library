namespace Trarizon.Library.Experimental.Interpreting.Brainfuck;
public interface IBrainfuckReader
{
    byte Read();
}

public interface IBrainfuckWriter
{
    void Write(byte value);
}

internal readonly struct BrainfuckTextReader(TextReader reader) : IBrainfuckReader
{
    public byte Read() => (byte)(reader?.Read() ?? throw new InvalidOperationException());
}

internal readonly struct BrainfuckTextWriter(TextWriter writer) : IBrainfuckWriter
{
    public void Write(byte value) => writer?.Write((char)value);
}
