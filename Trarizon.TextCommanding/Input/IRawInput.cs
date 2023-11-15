namespace Trarizon.TextCommanding.Input;
internal interface IRawInput
{
    bool MoveNext();
    InputSplit Current { get; }
}
