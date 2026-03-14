namespace Trarizon.Library.Experimental.Interpreting.Brainfuck;
internal ref struct BrainfuckExpressionReader(ReadOnlySpan<char> expression, Span<int> loopStack)
{
    private readonly ReadOnlySpan<char> _expression = expression;
    private int _index;
    private readonly Span<int> _loopStack = loopStack;
    private int _loopDepth;

    public void EnterLoop()
    {
        if (_loopDepth >= _loopStack.Length)
            throw new BrainfuckException("Stack overflow");
        _loopStack[_loopDepth] = _index;
        _loopDepth++;
    }

    public void ExitLoop()
    {
        if (_loopDepth == 0)
            throw new BrainfuckException("Excepted start of loop.");
        _loopDepth--;
    }

    public void Loop()
    {
        if (_loopDepth == 0)
            throw new BrainfuckException("Excepted start of loop.");
        _index = _loopStack[_loopDepth - 1];
    }

    public bool TryReadNext(out char ch)
    {
        if (_index >= _expression.Length) {
            if (_loopDepth > 0)
                throw new BrainfuckException("Excepted end of loop");
            ch = default;
            return false;
        }

        ch = _expression[_index++];
        return true;
    }

    public bool ReadUntil(char ch)
    {
        while (_expression[_index] != ch) {
            _index++;
            if (_index >= _expression.Length)
                return false;
        }
        return true;
    }
}
