namespace Trarizon.Library.Experimental.Interpreting.Brainfuck;
internal ref struct BrainfuckMemory(Span<byte> memory)
{
    private readonly Span<byte> _memory = memory;
    private int _ptr;

    public int Pointer
    {
        readonly get => _ptr;
        set {
            if (_ptr < 0 || _ptr >= _memory.Length)
                throw new BrainfuckException("Pointer out of range");
            _ptr = value;
        }
    }


    public readonly ref byte CurrentData => ref _memory[Pointer];
}
